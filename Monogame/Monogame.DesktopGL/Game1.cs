using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using XnaColor = Microsoft.Xna.Framework.Color;
using static Character;
using RogueTest;
using System.Numerics;



namespace RoguelikeMonoGame
{
    public class Game1 : Game
    {
        // ====== Map / Layout ======
        const int TileSize = 24;
        const int MapWidth = 50;
        const int MapHeight = 30;

        // Camera is in tile coordinates (top-left of the visible map)
        Camera2D camera;

        World _world;
        readonly Dictionary<Guid, Point> _npcLastKnown = new();
        readonly Dictionary<Guid, Point> _npcLastSeen = new();
        HashSet<Guid> _visibleNpcIds = new();




        // UI: toolbar + right panel (same layout as before)
        const int ToolbarWidth = 56;
        const int PanelWidth = 320;
        int LeftUIWidth => ToolbarWidth + PanelWidth;
        int BackbufferW => LeftUIWidth + MapWidth * TileSize;
        int BackbufferH => MapHeight * TileSize;
        RangedTargetingState _ranged = new RangedTargetingState();
        private TileRenderer _tileRenderer;

        private VisionSystem _vision;
        private DoorSystem _doorSystem = new DoorSystem();
        // ====== MonoGame ======
        GraphicsDeviceManager _gdm;
        SpriteBatch _sb;
        Texture2D _px;
        FontSystem _font;

        KeyboardState _ks, _pks;
        MouseState _ms, _pms;

        readonly Random _rng = new();



        // ====== Game State ======
        DungeonMap _map;
        PlayerCharacter _player;
        List<NonPlayerCharacter> _enemies = new();
        int _prevMouseScroll = 0;

        int _level = 1;
        bool _gameOver;

        // Auto-walk & dialogs
        List<Point> _autoPath = new();
        bool _showInventory = false;
        int _invIndex = 0;

        // UI
        enum PanelView { Main, P2, P3, P4 }
        PanelView _active = PanelView.Main;

        private Point AheadOfPlayer(int dist = 1)
        {
            return new Point(
                _player.Pos.X + _player.Facing.X * dist,
                _player.Pos.Y + _player.Facing.Y * dist);
        }
        public Game1()
        {
            _gdm = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Optional: window / backbuffer settings
            //_gdm.PreferredBackBufferWidth = 1280;
            //_gdm.PreferredBackBufferHeight = 720;
            //_gdm.ApplyChanges();
        }
        bool TryInteract()
        {

            // Doors & secret doors in front
            if (_doorSystem.TryInteractAhead(_player, _map, _enemies))
            {
                _vision.Recompute(_map, _player, _enemies, _map.ItemsAt);
                return true;
            }

            // Items under the player
            if (_map.ItemsAt.TryGetValue(_player.Pos, out var itemsHere) && itemsHere.Count > 0)
            {
                itemsHere[^1].Interact(_player, _map, _enemies, _map.ItemsAt);
                if (itemsHere.Count == 0)
                    _map.ItemsAt.Remove(_player.Pos);

                _vision.Recompute(_map,_player,_enemies, _map.ItemsAt);
                return true;
            }

            return false;
        }

        protected override void Initialize()
        {
            Window.Title = "MonoGame Roguelike — Data-driven Items & Uses";
            camera = new Camera2D();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _px = new Texture2D(GraphicsDevice, 1, 1);
            _px.SetData(new[] { XnaColor.White });
            _tileRenderer = new TileRenderer(_sb, TileSize, LeftUIWidth);
            _tileRenderer.FillRect = (rect, col) => FillRect(rect, col);          // your existing helper
            _tileRenderer.DrawText = (txt, x, y, col, size) => DrawText(txt, x, y, col, size);

            _font = new FontSystem();
            string[] winFonts =
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "calibri.ttf")
            };
            bool loaded = false;
            foreach (var f in winFonts)
                if (File.Exists(f)) { _font.AddFont(File.ReadAllBytes(f)); loaded = true; break; }
            if (!loaded)
            {
                var local = Path.Combine(AppContext.BaseDirectory, "LocalFont.ttf");
                if (File.Exists(local)) { _font.AddFont(File.ReadAllBytes(local)); loaded = true; }
            }
            if (!loaded) throw new FileNotFoundException("No TTF found. Place 'LocalFont.ttf' next to the EXE.");
            TilesetLibrary.Load(Content);
            SpriteFactory.Load(Content);
            ItemRegistry.BootstrapDefaults();  // inline defs for torch+sword (swap to JSON loader later)
            _world = new World(worldW: 1, worldH: 1, regionW: MapWidth, regionH: MapHeight, rng: _rng);
            _vision = new VisionSystem(MapWidth, MapHeight);

            NewGameFromWorld();
        }
        Tileset CurrentTileset =>
    TilesetLibrary.Tilesets[_world.State.CurrentLevel.Theme];
        void NewGameFromWorld()
        {
            _gameOver = false;
            _enemies.Clear();
            _npcLastSeen.Clear();
            _npcLastKnown.Clear();
            _visibleNpcIds.Clear();
            _autoPath.Clear();
            _showInventory = false;

            var level = _world.State.CurrentLevel;
            if (level == null) return;

            _map = new DungeonMap(MapWidth, MapHeight);
            _map.LoadFromWalkable(level.Walkable);
            BuildTilesFromWalkable(level);
            _player = new PlayerCharacter(level.PlayerStart)
            {
                HP = 30,
                MaxHP = 30
            };
            CenterCameraOnPlayer();
            if (!_map.IsPassable(_player.Pos))
            {
                _player.Pos = _map.RandomFloorNotOccupied(_rng,_player.Pos, _enemies); // pick a walkable tile
            }
            System.Diagnostics.Debug.WriteLine($"Player start: {_player.Pos}");
            _player.Explored = new bool[MapWidth, MapHeight];
            _player.Visible = new bool[MapWidth, MapHeight];

            GiveDefaultStartingItems();
            SpawnEnemies();
            //DEBUG
            _player.Visible = new bool[_map.Width, _map.Height];
            _player.Explored = new bool[_map.Width, _map.Height];
            for (int y = 0; y < _map.Height; y++)
                for (int x = 0; x < _map.Width; x++)
                {
                    _player.Visible[x, y] = true;
                    _player.Explored[x, y] = true;
                }

            RecomputeAllFov();
        }

        void BuildTilesFromWalkable(Level level)
        {
            var w = level.Walkable.GetLength(0);
            var h = level.Walkable.GetLength(1);

            var tiles = new TileCell[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool walkable = level.Walkable[x, y];
                    ref TileCell t = ref tiles[x, y];

                    if (walkable)
                    {
                        // floor
                        t.Ground = GroundType.Stone;
                        t.GroundVariant = 0;
                        t.Wall = null;
                    }
                    else
                    {
                        // wall
                        t.Ground = GroundType.Stone;
                        t.GroundVariant = 0;
                        t.Wall = WallType.Brick;
                        t.WallVariant = 0;
                    }
                }
            }

            level.Tiles = tiles;
            // After filling level.Tiles[,] from Walkable[]
            if (level.Connections != null)
            {
                foreach (var conn in level.Connections)
                {
                    // Only decorate tiles that originate on this level
                    if (conn.FromLevelId != level.Id) continue;

                    var p = conn.FromPos;
                    if (p.X < 0 || p.Y < 0 ||
                        p.X >= level.Tiles.GetLength(0) ||
                        p.Y >= level.Tiles.GetLength(1))
                        continue;

                    ref TileCell t = ref level.Tiles[p.X, p.Y];

                    switch (conn.Type)
                    {
                        case ConnectionType.BuildingEntrance:
                            // Make it a wooden floor with a wood wall (door-ish)
                            t.Ground = GroundType.Wood;
                            t.Wall = WallType.Wood;
                            break;

                        case ConnectionType.StairsDown:
                        case ConnectionType.StairsUp:
                            // e.g. use stone floor, no wall, and maybe a special marker later
                            t.Ground = GroundType.Stone;
                            t.Wall = null;
                            break;

                        case ConnectionType.Portal:
                            // Maybe highlight portals differently
                            t.Ground = GroundType.Stone;
                            t.Wall = WallType.Brick;
                            break;
                    }
                }
            }


        }
        void GiveDefaultStartingItems()
        {
            _player.Inventory.Clear();
            _player.Equipped.Clear();

            var torch = ItemFactory.CreateLightInInventory("torch");
            var sword = ItemFactory.CreateWeaponInInventory("sword");

            _player.Inventory.Add(torch);
            _player.Inventory.Add(sword);

            _player.Equip(torch, torch.Slot);   // "Light"
            _player.Equip(sword, sword.Slot);   // "Weapon"
        }

        void SpawnEnemies()
        {
            _enemies.Clear();
            var ai = new NpcAI(NpcBehavior.Hostile);

            int enemyCount = 7 + _level * 2;
            for (int i = 0; i < enemyCount; i++)
            {
                var monsterSeed = _rng.Next(100);
                NpcKind kind = NpcKind.Orc;
                if (monsterSeed < 30)
                    kind = NpcKind.SkeletonArcher;
                else if (monsterSeed < 60)
                    kind = NpcKind.Dragon;
                Point p;
                if (kind == NpcKind.Dragon)
                    p = _map.RandomAreaStart(_rng, 2, 2, _enemies);
                else 
                    p = _map.RandomFloorNotOccupied(_rng, _player.Pos, _enemies);
                var npc = new NonPlayerCharacter(p, kind, ai);

                if (kind == NpcKind.Orc)
                    SpriteFactory.SetupOrcAnimations(npc);
                else if (kind == NpcKind.SkeletonArcher)
                    SpriteFactory.SetupSkeletonArcherAnimations(npc);

                // Give archers a bow; melee get a sword (data-driven)
                var wid = (kind == NpcKind.SkeletonArcher) ? "bow" : "sword";
                if (ItemRegistry.Weapons.TryGetValue(wid, out var wdef))
                {
                    var w = new WeaponItem(wdef);
                    npc.Inventory.Add(w);
                    npc.EquippedWeapon = w;
                }

                _enemies.Add(npc);
            }
            var offsets = new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1) };

            foreach (var off in offsets)
            {
                var p = _player.Pos + off;
                if (!_map.InBounds(p) || !_map.IsPassable(p)) continue;
                var npc = new NonPlayerCharacter(p, NpcKind.Orc, ai);
                SpriteFactory.SetupOrcAnimations(npc);
                _enemies.Add(npc);
            }

            // Recompute vision so they show up immediately
            _vision.Recompute(_map, _player, _enemies, _map.ItemsAt);
        }

        // ====== New Level ======
        int Get8WayMask<T>(T[,] grid, int x, int y) where T : struct
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            T here = grid[x, y];

            bool Same(int xx, int yy)
            {
                if (xx < 0 || yy < 0 || xx >= w || yy >= h) return false;
                return EqualityComparer<T>.Default.Equals(grid[xx, yy], here);
            }

            int mask = 0;
            if (Same(x, y - 1)) mask |= 1;   // N
            if (Same(x + 1, y - 1)) mask |= 2;   // NE
            if (Same(x + 1, y)) mask |= 4;   // E
            if (Same(x + 1, y + 1)) mask |= 8;   // SE
            if (Same(x, y + 1)) mask |= 16;  // S
            if (Same(x - 1, y + 1)) mask |= 32;  // SW
            if (Same(x - 1, y)) mask |= 64;  // W
            if (Same(x - 1, y - 1)) mask |= 128; // NW

            return mask;
        }
        int GetWaterEdgeMask8(int x, int y, GroundType[,] ground)
        {
            int w = ground.GetLength(0);
            int h = ground.GetLength(1);

            bool IsWater(int xx, int yy)
            {
                if (xx < 0 || yy < 0 || xx >= w || yy >= h) return false;
                return ground[xx, yy] == GroundType.Water;
            }

            bool IsLand(int xx, int yy)
            {
                if (xx < 0 || yy < 0 || xx >= w || yy >= h) return false;
                return ground[xx, yy] != GroundType.Water;
            }

            // only edge-mask water tiles
            if (!IsWater(x, y)) return 0;

            int mask = 0;
            if (IsLand(x, y - 1)) mask |= 1;   // N
            if (IsLand(x + 1, y - 1)) mask |= 2;   // NE
            if (IsLand(x + 1, y)) mask |= 4;   // E
            if (IsLand(x + 1, y + 1)) mask |= 8;   // SE
            if (IsLand(x, y + 1)) mask |= 16;  // S
            if (IsLand(x - 1, y + 1)) mask |= 32;  // SW
            if (IsLand(x - 1, y)) mask |= 64;  // W
            if (IsLand(x - 1, y - 1)) mask |= 128; // NW

            return mask;
        }
        void ApplyRangedAttack()
        {
            var affected = new List<Point>();

            switch (_ranged.Shape)
            {
                case RangedShapeKind.SingleCell:
                    affected.Add(_ranged.TargetCell);
                    break;

                case RangedShapeKind.Circle:
                    affected.AddRange(GetCircle(_ranged.TargetCell, _ranged.Radius));
                    break;

                case RangedShapeKind.Cone:
                    affected.AddRange(GetCone(_player.Pos, _ranged.Direction, _ranged.Radius));
                    break;

                case RangedShapeKind.Line:
                    affected.AddRange(GetLine(_player.Pos, _ranged.Direction, _ranged.Range));
                    break;
            }

            // Apply damage to enemies in affected cells
            foreach (var e in _enemies)
            {
                if (!affected.Contains(e.Pos)) continue;

                e.TakeDamage(_ranged.Damage);
            }


            // Optionally: friendly fire – check if player is in affected
            // if (affected.Contains(_player.Pos)) _player.TakeDamage(...);

            // Optionally: mark map cells as scorched / apply effects, etc.

            // If your WeaponItem has a use system (PerformUse), you could call:
            // _ranged.Weapon?.PerformUse(_ranged.UseId, _player, _map, _enemies, null, Point.Zero, _rng);
        }
        IEnumerable<Point> GetCircle(Point center, int radius)
        {
            var list = new List<Point>();
            int r2 = radius * radius;
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
                for (int x = center.X - radius; x <= center.X + radius; x++)
                {
                    var p = new Point(x, y);
                    if (!_map.InBounds(p)) continue;
                    int dx = x - center.X;
                    int dy = y - center.Y;
                    if (dx * dx + dy * dy <= r2)
                        list.Add(p);
                }
            return list;
        }

        IEnumerable<Point> GetLine(Point origin, Point dir, int length)
        {
            var list = new List<Point>();
            var p = origin;
            for (int i = 1; i <= length; i++)
            {
                p = new Point(p.X + dir.X, p.Y + dir.Y);
                if (!_map.InBounds(p)) break;
                list.Add(p);
                if (_map.IsOpaque(p)) break; // lightning stops at walls
            }
            return list;
        }

        IEnumerable<Point> GetCone(Point origin, Point dir, int length)
        {
            var list = new List<Point>();
            // simple cone: all cells within 'length' whose direction is within ~45 degrees of dir
            float centerDeg = Character.DegreesFromDir(dir);
            float halfAngle = 45f;
            int r2 = length * length;

            for (int y = origin.Y - length; y <= origin.Y + length; y++)
                for (int x = origin.X - length; x <= origin.X + length; x++)
                {
                    var p = new Point(x, y);
                    if (!_map.InBounds(p)) continue;
                    int dx = x - origin.X;
                    int dy = y - origin.Y;
                    int d2 = dx * dx + dy * dy;
                    if (d2 == 0 || d2 > r2) continue;

                    float ang = Character.DegreesFromDir(new Point(dx, dy));
                    float delta = AngleDelta(centerDeg, ang);
                    if (Math.Abs(delta) <= halfAngle)
                    {
                        // optional: LOS check so cone doesn't go through walls
                        if (_map.HasLineOfSight(origin, p))
                            list.Add(p);
                    }
                }

            return list;
        }

        float AngleDelta(float a, float b)
        {
            float d = (b - a + 540f) % 360f - 180f;
            return d;
        }
        int GetWallMask8(int x, int y, WallType[,] walls)
        {
            return Get8WayMask(walls, x, y);
        }

        int GetGroundMask8(int x, int y, GroundType[,] ground)
        {
            return Get8WayMask(ground, x, y);
        }



        // ====== Update ======
        protected override void Update(GameTime gameTime)
        {
            _ks = Keyboard.GetState();
            _ms = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update all unique animated tiles once
            foreach (var anim in TileAnimationLibrary.AnimatedTiles.Values)
                anim.Update(dt);

            // Also update NPC animations
            foreach (var npc in _enemies)
                npc.UpdateAnimation(dt);

            base.Update(gameTime);
            if (Pressed(Keys.Escape))
            {
                if (_showInventory) _showInventory = false;
                else Exit();
            }

            if (_gameOver)
            {
                if (Pressed(Keys.Enter)) { _level = 1; NewGameFromWorld(); }
                _pks = _ks; _pms = _ms;  
                return;
            }
            if (Pressed(Keys.E))
            {
                if (TryInteract())
                {
                    _world.AdvanceTurn(_map, _player, _enemies, _rng);   // interacting costs a turn
                    RecomputeAllFov();
                    _autoPath = null;
                    return;
                }
            }
            // Inventory
            if (Pressed(Keys.I)) { _showInventory = !_showInventory; _invIndex = 0; }

            if (_showInventory)
            {
                HandleInventory();
                _pks = _ks; _pms = _ms; 
                return;
            }

            bool acted = false;

            // Click-to-move if no enemy visible
            if (Clicked() && _ms.X >= LeftUIWidth)
            {
                var g = ScreenToGrid(_ms.Position);
                if (_map.InBounds(g) && (_player.Visible[g.X, g.Y] || _player.Explored[g.X, g.Y]))
                {
                    if (!AnyEnemyVisible())
                        _autoPath = _world.FindPath(_map, _player.Pos, g, _enemies);
                }
            }

            // Movement
            Point dir = Point.Zero;
            if (Pressed(Keys.Left)) dir = new Point(-1, 0);
            if (Pressed(Keys.Right)) dir = new Point(1, 0);
            if (Pressed(Keys.Up)) dir = new Point(0, -1);
            if (Pressed(Keys.Down)) dir = new Point(0, 1);
            
            if (dir != Point.Zero)
            {
                _player.TryMove(dir, _map, _world, _enemies, _rng);
                acted = true;
                _autoPath = null;
                camera.Follow(_player.Pos,
              GraphicsDevice.Viewport.Width,
              GraphicsDevice.Viewport.Height,
              levelWidthInPixels,
              levelHeightInPixels);
            }

            // Attack / primary action (F)
            // Start ranged targeting with F
            if (!_ranged.Active && _ks.IsKeyDown(Keys.F))
            {
                TryStartRangedTargeting();
            }

            // If we are in targeting mode, handle it separately
            if (_ranged.Active)
            {
                HandleRangedTargetingInput(_ks, _ms);
                return; // prevent normal movement while aiming
            }

            // Wait turn
            if (Pressed(Keys.Space) || Pressed(Keys.OemPeriod) || Pressed(Keys.NumPad5))
                acted = true;

            // Auto-walk step
            if (!acted && _autoPath != null && _autoPath.Count > 0)
            {
                if (AnyEnemyVisible()) _autoPath = null;
                else
                {
                    var next = _autoPath[0];
                    var step = new Point(Math.Sign(next.X - _player.Pos.X), Math.Sign(next.Y - _player.Pos.Y));
                    if (step != Point.Zero)
                    {
                        var before = _player.Pos;
                        _player.TryMove(step, _map, _world, _enemies, _rng);
                        if (_player.Pos == next) _autoPath.RemoveAt(0);
                        acted = true;
                    }
                }
            }

            // Stairs -> new level
            if (Pressed(Keys.Enter))
            {
                TryUseConnection();
            }

            // Enemy/projectile turns
            if (acted)
            {
                _world.AdvanceTurn(_map, _player, _enemies, _rng);
                RecomputeAllFov();

                if (_player.IsDead)
                {
                    _gameOver = true;
                    _pks = _ks;
                    _pms = _ms;
                    return;
                }
            }

            _pks = _ks; _pms = _ms;
        }
        void TryStartRangedTargeting()
        {
            // Basic example: use the player's equipped weapon if it has a ranged use.
            // Adapt to your actual weapon/uses system.
            if (_player.EquippedWeapon is not WeaponItem w)
                return;

            // Decide which use / shape from the weapon.
            // For now, keep it simple and infer from weapon name or a tag.
            // You probably already have UseIds like "Shoot", "Fireball" etc.
            string useId;
            RangedShapeKind shape;
            int range;
            int radius;
            int damage = w.Damage; // or look up from a specific use

            // You can make this data-driven later.
            if (w.Tags.Contains("Fireball"))
            {
                useId = "Fireball";
                shape = RangedShapeKind.Circle;
                range = 6;
                radius = 2; // fireball radius
            }
            else if (w.Tags.Contains("ConeOfCold"))
            {
                useId = "ConeOfCold";
                shape = RangedShapeKind.Cone;
                range = 6;
                radius = 5;
            }
            else if (w.Tags.Contains("Lightning"))
            {
                useId = "Lightning";
                shape = RangedShapeKind.Line;
                range = 8;
                radius = 8;
            }
            else
            {
                // default: bow/arrow style single cell
                useId = "Shoot";
                shape = RangedShapeKind.SingleCell;
                range = 8;
                radius = 0;
            }

            _ranged.Active = true;
            _ranged.Shape = shape;
            _ranged.Range = range;
            _ranged.Radius = radius;
            _ranged.Origin = _player.Pos;
            _ranged.TargetCell = _player.Pos + _player.Facing;
            _ranged.Direction = _player.Facing;   // for cone/line
            _ranged.Weapon = w;
            _ranged.UseId = useId;
            _ranged.Damage = damage;
        }
        void HandleRangedTargetingInput(KeyboardState kb, MouseState mouse)
        {
            // 1) Cancel with Esc
            if (Pressed(Keys.Escape))
            {
                _ranged.Active = false;
                return;
            }

            // 2) Update target from mouse position
            var cell = ScreenToMapCell(mouse.X, mouse.Y);
            if (cell.HasValue)
            {
                var p = cell.Value;

                switch (_ranged.Shape)
                {
                    case RangedShapeKind.SingleCell:
                    case RangedShapeKind.Circle:
                        _ranged.TargetCell = p;
                        break;

                    case RangedShapeKind.Cone:
                    case RangedShapeKind.Line:
                        // Direction from player to mouse cell
                        var dx = p.X - _player.Pos.X;
                        var dy = p.Y - _player.Pos.Y;
                        if (dx != 0 || dy != 0)
                        {
                            // normalize to cardinal / diagonal direction
                            var dir = new Point(Math.Sign(dx), Math.Sign(dy));
                            if (dir.X != 0 || dir.Y != 0)
                                _ranged.Direction = dir;
                        }
                        break;
                }
            }

            // 3) Mouse wheel rotates direction for cone/line
            int deltaScroll = mouse.ScrollWheelValue - _prevMouseScroll;
            if (deltaScroll != 0 && (_ranged.Shape == RangedShapeKind.Cone || _ranged.Shape == RangedShapeKind.Line))
            {
                RotateRangedDirection(deltaScroll > 0);
            }
            _prevMouseScroll = mouse.ScrollWheelValue;

            // 4) Confirm with Enter
            if (Pressed(Keys.Enter))
            {
                ApplyRangedAttack();
                _ranged.Active = false;
                // Ranged attack consumes a turn
                _world.AdvanceTurn(_map, _player, _enemies, _rng);
                RecomputeAllFov();
            }
        }
        Point? ScreenToMapCell(int mouseX, int mouseY)
        {
            int ox = LeftUIWidth;
            int x = (mouseX - ox) / TileSize;
            int y = mouseY / TileSize;
            if (x < 0 || y < 0 || x >= MapWidth || y >= MapHeight) return null;
            return new Point(x, y);
        }
        void RotateRangedDirection(bool clockwise)
        {
            // 8 directions
            var dirs = new[]
            {
        new Point( 1, 0), // E
        new Point( 1, 1), // SE
        new Point( 0, 1), // S
        new Point(-1, 1), // SW
        new Point(-1, 0), // W
        new Point(-1,-1), // NW
        new Point( 0,-1), // N
        new Point( 1,-1), // NE
    };

            int idx = Array.FindIndex(dirs, d => d == _ranged.Direction);
            if (idx < 0) idx = 0;

            if (clockwise) idx = (idx + 1) % dirs.Length;
            else idx = (idx - 1 + dirs.Length) % dirs.Length;

            _ranged.Direction = dirs[idx];
        }
        void HandleInventory()
        {
            if (Pressed(Keys.Up)) _invIndex = Math.Max(0, _invIndex - 1);
            if (Pressed(Keys.Down)) _invIndex = Math.Min(Math.Max(0, _player.Inventory.Count - 1), _invIndex + 1);

            if (_player.Inventory.Count == 0) return;
            var item = _player.Inventory[_invIndex];

            // Equip/Unequip (Enter)
            if (Pressed(Keys.Enter))
            {
                if (item.CanEquip)
                {
                    // Toggle equip in its declared slot
                    if (_player.Equipped.TryGetValue(item.Slot, out var cur) && cur == item)
                        _player.Unequip(item.Slot);
                    else
                        _player.Equip(item, item.Slot);

                    _vision.Recompute(_map, _player, _enemies, _map.ItemsAt);
                }
            }
            if (Pressed(Keys.S))
            {
                bool found = _player.Search(_map, radius: 2);
                // Optionally show a message if found == true/false here.

                _world.AdvanceTurn(_map, _player, _enemies, _rng);
                RecomputeAllFov();
                return;
            }

            // Drop (D)
            if (Pressed(Keys.D))
            {
                var drop = item;
                // If equipped in its slot, unequip first
                if (_player.Equipped.TryGetValue(drop.Slot, out var cur) && cur == drop)
                    _player.Unequip(drop.Slot);

                drop.OnDropped(_player);
                drop.Pos = _player.Pos;
                if (!_map.ItemsAt.TryGetValue(drop.Pos, out var list)) { list = new List<Item>(); _map.ItemsAt[drop.Pos] = list; }
                list.Add(drop);
                _player.Inventory.RemoveAt(_invIndex);
                if (_invIndex >= _player.Inventory.Count) _invIndex = Math.Max(0, _player.Inventory.Count - 1);

                _vision.Recompute(_map, _player, _enemies, _map.ItemsAt);
            }
        }
        bool TryUseConnection()
        {
            var level = _world.State.CurrentLevel;
            if (level == null) return false;

            var conn = level.FindConnectionAt(_player.Pos);
            if (conn == null)
            {
                // Optional: temporary debug log
                // Console.WriteLine($"No connection at {_player.Pos} on level {level.Id}");
                return false;
            }

            var region = _world.State.CurrentRegion;
            var targetLevel = region.GetLevelById(conn.ToLevelId);
            if (targetLevel == null) return false;

            _world.State.CurrentLevel = targetLevel;

            // Rebuild DungeonMap from this Level
            _map = new DungeonMap(MapWidth, MapHeight);
            _map.LoadFromWalkable(targetLevel.Walkable);

            _player.Pos = conn.ToPos;

            // Re-spawn enemies/items if needed
            _enemies.Clear();
            SpawnEnemies();

            _vision.Recompute(_map, _player, _enemies, _map.ItemsAt);
            return true;
        }
        void UpdateLastKnown()
        {
            var newVisible = new HashSet<Guid>();

            foreach (var e in _enemies)
            {
                if (!_map.InBounds(e.Pos)) continue;

                if (_player.Visible[e.Pos.X, e.Pos.Y])
                {
                    newVisible.Add(e.Id);
                    _npcLastSeen[e.Id] = e.Pos;
                    _npcLastKnown.Remove(e.Id);
                }
            }

            // Any enemy that was visible last frame but not this frame:
            foreach (var id in _visibleNpcIds)
            {
                if (!newVisible.Contains(id) && _npcLastSeen.TryGetValue(id, out var pos))
                    _npcLastKnown[id] = pos;
            }

            _visibleNpcIds = newVisible;
        }
        void RecomputeAllFov()
        {
            if (_vision == null || _map == null || _player == null)
                return;
            _vision.Recompute(_map, _player, _enemies, _map.ItemsAt);
            UpdateLastKnown();
            CleanNpcTracking();
        }
        void CleanNpcTracking()
        {
            var alive = new HashSet<Guid>();
            foreach (var e in _enemies)
                alive.Add(e.Id);

            foreach (var id in new List<Guid>(_npcLastKnown.Keys))
                if (!alive.Contains(id)) _npcLastKnown.Remove(id);

            foreach (var id in new List<Guid>(_npcLastSeen.Keys))
                if (!alive.Contains(id)) _npcLastSeen.Remove(id);

            _visibleNpcIds.RemoveWhere(id => !alive.Contains(id));
        }

        // ====== Draw ======
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(XnaColor.Black);
            _sb.Begin(transformMatrix: camera.Transform);
            int ox = LeftUIWidth;
            // Toolbar
            FillRect(new Rectangle(0, 0, ToolbarWidth, BackbufferH), new XnaColor(28, 28, 30));
            DrawToolButton(0, "Main");
            DrawToolButton(1, "P2");
            DrawToolButton(2, "P3");
            DrawToolButton(3, "P4");

            // Panel
            FillRect(new Rectangle(ToolbarWidth, 0, PanelWidth, BackbufferH), new XnaColor(20, 20, 20));
            switch (_active)
            {
                case PanelView.Main: DrawMainPanel(); break;
                default: DrawPanelTitle(_active.ToString()); break;
            }
            var level = _world.State.CurrentLevel;   // we already use this a bit later

            if (level != null)
            {
                _tileRenderer.DrawWorld(
        _map,
        level,
        CurrentTileset,
        _player,// TilesetLibrary.Tilesets[level.Theme]
        _enemies,
        _map.ItemsAt,
        _vision.Light);

                foreach (var conn in level.Connections)
                {
                    var p = conn.FromPos;
                    if (!_map.InBounds(p)) continue;
                    if (!_player.Visible[p.X, p.Y]) continue;

                    var r = new Rectangle(ox + p.X * TileSize, p.Y * TileSize, TileSize, TileSize);

                    // Choose glyph/color based on type
                    string glyph = ">";
                    XnaColor color = new XnaColor(15, 15, 25);

                    switch (conn.Type)
                    {
                        case ConnectionType.StairsUp:
                            glyph = "<";
                            color = new XnaColor(25, 15, 25);
                            break;
                        case ConnectionType.StairsDown:
                            glyph = ">";
                            color = new XnaColor(15, 15, 25);
                            break;
                        case ConnectionType.Portal:
                            glyph = "#";
                            color = new XnaColor(60, 60, 90);
                            break;
                            // extend with other types as you add them
                    }

                    FillRect(r, color);
                    DrawText(glyph, r.X + 6, r.Y + 2, XnaColor.LightSteelBlue, 18);
                }
            }

            // ==========================
            // DOORS (from _map.Doors)
            // ==========================
            foreach (var door in _map.Doors.Values)
            {
                var p = door.Pos;

                // hidden secret doors look like walls; don't draw them as doors
                if (door is SecretDoorObject sd && !sd.Discovered)
                    continue;

                if (!_player.Visible[p.X, p.Y])
                    continue;

                var r = new Rectangle(ox + p.X * TileSize, p.Y * TileSize, TileSize, TileSize);

                // slight background tint
                FillRect(r, new XnaColor(60, 50, 30));

                var glyphColor = door.State == DoorState.Locked
                    ? XnaColor.Goldenrod
                    : XnaColor.SandyBrown;

                DrawText(door.Glyph, r.X + 6, r.Y + 2, glyphColor, 18);
            }

            // ==========================
            // ITEMS (skip doors)
            // ==========================
            foreach (var kv in _map.ItemsAt)
            {
                var p = kv.Key;
                var list = kv.Value;

                if (!_player.Visible[p.X, p.Y]) continue;

                // hide hidden items
                var visibleItems = list.Where(it =>
                    it is not IHiddenRevealable h || h.Discovered).ToList();

                if (visibleItems.Count == 0)
                    continue;

                // remove any doors defensively; doors are drawn from _map.Doors
                var nonDoorItems = visibleItems.Where(it => it is not DoorObject).ToList();
                if (nonDoorItems.Count == 0)
                    continue;

                var r = new Rectangle(ox + p.X * TileSize, p.Y * TileSize, TileSize, TileSize);

                if (nonDoorItems.Count == 1)
                {
                    DrawText(nonDoorItems[0].Glyph, r.X + 6, r.Y + 2, XnaColor.Gold, 18);
                }
                else
                {
                    // item pile representation
                    FillRect(new Rectangle(r.X + 6, r.Y + 10, 12, 6), new XnaColor(120, 90, 40));
                    DrawText(nonDoorItems.Count.ToString(), r.X + 4, r.Y + 1, XnaColor.Wheat, 14);
                }
            }

            if (_ranged.Active)
            {
                DrawRangedOverlay();
            }

            // Enemies (visible)
            foreach (var e in _enemies)
            {
                if (!_map.InBounds(e.Pos)) continue;
                if (!_player.Visible[e.Pos.X, e.Pos.Y]) continue;

                var r = new Rectangle(
                    LeftUIWidth + e.Pos.X * TileSize,
                    e.Pos.Y * TileSize,
                    TileSize,
                    TileSize);

                // Different colors per kind if you like
                var col = e.Kind == NpcKind.SkeletonArcher
                    ? new XnaColor(80, 130, 60)
                    : new XnaColor(200, 40, 40);

                DrawText(e.Glyph ?? "m", r.X + 6, r.Y + 2, col, 18);
            }

            // Last-known enemy markers stay as they are…

            foreach (var kv in _npcLastKnown)
            {
                var p = kv.Value;
                if (!_map.InBounds(p)) continue;

                if (_player.Explored == null || !_player.Explored[p.X, p.Y]) continue;
                if (_player.Visible != null && _player.Visible[p.X, p.Y]) continue;

                var r = new Rectangle(LeftUIWidth + p.X * TileSize, p.Y * TileSize, TileSize, TileSize);
                DrawText("?", r.X + 7, r.Y + 3, new XnaColor(180, 180, 220, 160), 18);
            }

            // Player – draw as '@'
            if (!_player.IsDead && _map.InBounds(_player.Pos))
            {
                if (_player.Visible == null || _player.Visible[_player.Pos.X, _player.Pos.Y])
                {
                    var pr = new Rectangle(
                        LeftUIWidth + _player.Pos.X * TileSize,
                        _player.Pos.Y * TileSize,
                        TileSize,
                        TileSize);

                    DrawText("@", pr.X + 6, pr.Y + 2, XnaColor.CornflowerBlue, 18);
                }
            }


            // Inventory modal
            if (_showInventory) DrawInventoryModal();

            // Game over
            if (_gameOver)
            {
                string msg = "Game Over — Press Enter";
                var f = _font.GetFont(22);
                var sz = f.MeasureString(msg);
                int cx = LeftUIWidth + (MapWidth * TileSize - (int)sz.X) / 2;
                int cy = (MapHeight * TileSize - (int)sz.Y) / 2;
                _sb.DrawString(f, msg, new Vector2(cx, cy), XnaColor.White);
            }


            _sb.End();
            base.Draw(gameTime);
        }

        // ====== Helpers ======
        bool Pressed(Keys k) => _ks.IsKeyDown(k) && !_pks.IsKeyDown(k);
        bool Clicked() => _ms.LeftButton == ButtonState.Pressed && _pms.LeftButton == ButtonState.Released;
        void DrawRangedOverlay()
        {
            var cells = new List<Point>();

            switch (_ranged.Shape)
            {
                case RangedShapeKind.SingleCell:
                    cells.Add(_ranged.TargetCell);
                    break;
                case RangedShapeKind.Circle:
                    cells.AddRange(GetCircle(_ranged.TargetCell, _ranged.Radius));
                    break;
                case RangedShapeKind.Cone:
                    cells.AddRange(GetCone(_player.Pos, _ranged.Direction, _ranged.Radius));
                    break;
                case RangedShapeKind.Line:
                    cells.AddRange(GetLine(_player.Pos, _ranged.Direction, _ranged.Range));
                    break;
            }

            // Color based on range: if the main target is out of range, show red; otherwise green.
            bool inRange = IsTargetInRange();
            var col = inRange ? new Color(0, 255, 0, 80) : new Color(255, 0, 0, 80);

            int ox = LeftUIWidth;
            foreach (var p in cells)
            {
                if (!_map.InBounds(p)) continue;
                var r = new Rectangle(ox + p.X * TileSize, p.Y * TileSize, TileSize, TileSize);
                FillRect(r, col);
            }
        }

        bool IsTargetInRange()
        {
            switch (_ranged.Shape)
            {
                case RangedShapeKind.SingleCell:
                case RangedShapeKind.Circle:
                    int dx = _ranged.TargetCell.X - _ranged.Origin.X;
                    int dy = _ranged.TargetCell.Y - _ranged.Origin.Y;
                    return (dx * dx + dy * dy) <= _ranged.Range * _ranged.Range;

                case RangedShapeKind.Cone:
                case RangedShapeKind.Line:
                    // For directional shapes, we typically just respect _ranged.Range as length
                    return true; // or add a more nuanced check if you like
            }
            return true;
        }

        Point ScreenToGrid(Point screen)
        {
            int gx = (screen.X - LeftUIWidth) / TileSize;
            int gy = (screen.Y) / TileSize;
            return new Point(gx, gy);
        }

        void FillRect(Rectangle r, XnaColor c) => _sb.Draw(_px, r, c);

        void DrawText(string text, int x, int y, XnaColor c, int size = 18)
        {
            var f = _font.GetFont(size);
            _sb.DrawString(f, text, new Vector2(x, y), c);
        }

        void DrawToolButton(int index, string label)
        {
            int topPad = 12, gap = 8, btnH = 48;
            int by = topPad + index * (btnH + gap);
            var r = new Rectangle(0, by, ToolbarWidth, btnH);
            bool sel = (int)_active == index;
            bool hover = r.Contains(_ms.Position);
            var bg = sel ? new XnaColor(80, 110, 180) : hover ? new XnaColor(55, 55, 65) : new XnaColor(40, 40, 46);
            FillRect(r, bg);

            var f = _font.GetFont(14);
            var sz = f.MeasureString(label);
            int tx = r.X + (r.Width - (int)sz.X) / 2;
            int ty = r.Y + (r.Height - (int)sz.Y) / 2;
            _sb.DrawString(f, label, new Vector2(tx, ty), XnaColor.White);

            if (Clicked() && r.Contains(_ms.Position)) _active = (PanelView)index;
        }

        void DrawPanelTitle(string text)
        {
            DrawText(text, ToolbarWidth + 16, 16, XnaColor.White, 20);
            DrawText("(empty)", ToolbarWidth + 16, 44, XnaColor.Silver, 16);
        }

        void DrawMainPanel()
        {
            int x = ToolbarWidth + 16;
            DrawText("MonoGame Roguelike", x, 10, XnaColor.White, 20);
            DrawText($"HP: {_player.HP}/{_player.MaxHP}", x, 48, XnaColor.Gainsboro, 18);

            // Equipped list
            int y = 78;
            DrawText("Equipped:", x, y, XnaColor.Silver, 16); y += 20;
            if (_player.Equipped.Count == 0) { DrawText("(none)", x, y, XnaColor.Gray, 16); y += 18; }
            else
            {
                foreach (var kv in _player.Equipped)
                {
                    DrawText($"{kv.Key}: {kv.Value.Name}", x, y, XnaColor.Gainsboro, 16);
                    y += 18;
                }
            }

            // Generator/level
            y += 8;
            DrawText($"Level: {_level}", x, y, XnaColor.LightSteelBlue, 16); y += 20;

            // Controls
            DrawText("WASD/Arrows: move  F: attack  I: inventory", x, y, XnaColor.Silver, 14); y += 18;
            DrawText("Click map to auto-walk; Space/.: wait", x, y, XnaColor.Silver, 14);
        }

        void DrawInventoryModal()
        {
            var r = new Rectangle(LeftUIWidth + 40, 40, MapWidth * TileSize - 80, MapHeight * TileSize - 80);
            FillRect(r, new XnaColor(18, 18, 22, 240));
            DrawText("Inventory  (Enter: equip/unequip, D: drop, Esc: close)", r.X + 16, r.Y + 12, XnaColor.White, 18);

            int y = r.Y + 48;
            if (_player.Inventory.Count == 0) { DrawText("(empty)", r.X + 24, y, XnaColor.Gainsboro, 18); return; }

            for (int i = 0; i < _player.Inventory.Count; i++)
            {
                var it = _player.Inventory[i];
                bool sel = (i == _invIndex);
                if (sel) FillRect(new Rectangle(r.X + 12, y - 2, r.Width - 24, 24), new XnaColor(70, 80, 95, 180));

                string extra = "";
                if (_player.Equipped.TryGetValue(it.Slot, out var cur) && cur == it) extra = " [equipped]";
                DrawText($"{it.Glyph}  {it.Name}{extra}", r.X + 24, y, sel ? XnaColor.Wheat : XnaColor.Gainsboro, 18);
                y += 26;
            }
        }
        bool AnyEnemyVisible() => _visibleNpcIds.Count > 0;
 

 
    }

}
