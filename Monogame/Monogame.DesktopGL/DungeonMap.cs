using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RogueTest;

namespace RoguelikeMonoGame
{
    /// <summary>
    /// Dungeon grid + doors + items + basic LOS helpers.
    /// All tile data lives in the Cell[,] array now.
    /// </summary>
    public class DungeonMap
    {
        // ---- Cell definition ------------------------------------------------
        public struct Cell
        {
            /// <summary>true if this tile is walkable floor.</summary>
            public bool IsFloor;
            // Later you can extend with: floor type, wall type, tile index etc.
        }

        // ---- Core data ------------------------------------------------------
        public int Width { get; }
        public int Height { get; }

        // All geometry is stored here
        private readonly Cell[,] _cells;

        public Point PlayerStart;
        public Point Stairs;

        public readonly Dictionary<Point, DoorObject> Doors = new();
        public Dictionary<Point, List<Item>> ItemsAt { get; } = new();
        public Dictionary<Point, Trap> Traps { get; } = new();
        // ---------------------------------------------------------------------
        // ctor & basic fill
        // ---------------------------------------------------------------------
        public DungeonMap(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new Cell[width, height];
        }
        public Trap? GetTrapAt(Point p)
    => Traps.TryGetValue(p, out var trap) ? trap : null;
        /// <summary>
        /// Replace dungeon layout from an external walkable grid (true = floor).
        /// </summary>
        public void LoadFromWalkable(bool[,] walkable)
        {
            int w = walkable.GetLength(0);
            int h = walkable.GetLength(1);

            for (int y = 0; y < Height && y < h; y++)
            {
                for (int x = 0; x < Width && x < w; x++)
                {
                    bool isFloor = walkable[x, y];
                    if (isFloor) SetFloor(x, y);
                    else SetWall(x, y);
                }
            }
        }

        // Small helpers to talk to the Cell[,] -------------------------------
        private void SetWall(int x, int y)
        {
            if (!InBounds(x, y)) return;
            _cells[x, y].IsFloor = false;
        }

        private void SetFloor(int x, int y)
        {
            if (!InBounds(x, y)) return;
            _cells[x, y].IsFloor = true;
        }

        internal void ForceCarveFloor(Point pos)
        {
            if (!InBounds(pos)) return;
            _cells[pos.X, pos.Y].IsFloor = true;
        }
        public bool InBounds(Rectangle area)
        {
            return area.Left >= 0 && area.Top >= 0 &&
                   area.Right <= Width && area.Bottom <= Height;
        }
        public bool IsAreaPassable(Rectangle area,
                           List<NonPlayerCharacter>? npcs = null,
                           NonPlayerCharacter? self = null)
        {
            if (!InBounds(area))
                return false;

            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    var p = new Point(x, y);

                    // Terrain / doors etc.
                    if (!IsPassable(p))
                        return false;

                    // Optional: avoid overlapping other NPCs
                    if (npcs != null)
                    {
                        foreach (var npc in npcs)
                        {
                            if (npc == self) continue;
                            if (npc.Occupies(p))
                                return false;
                        }
                    }
                }
            }

            return true;
        }
        public Point RandomAreaStart(Random rng, int sizeW, int sizeH,
                             List<NonPlayerCharacter>? npcs = null)
        {
            while (true)
            {
                int x = rng.Next(0, Width - sizeW);
                int y = rng.Next(0, Height - sizeH);
                var rect = new Rectangle(x, y, sizeW, sizeH);

                if (IsAreaPassable(rect, npcs, null))
                    return new Point(x, y);
            }
        }
        void CarveHorizontal(int x1, int x2, int y)
        {
            if (y < 0 || y >= Height) return;
            if (x2 < x1) (x1, x2) = (x2, x1);
            for (int x = x1; x <= x2; x++)
                if (InBounds(x, y)) SetFloor(x, y);
        }

        void CarveVertical(int y1, int y2, int x)
        {
            if (x < 0 || x >= Width) return;
            if (y2 < y1) (y1, y2) = (y2, y1);
            for (int y = y1; y <= y2; y++)
                if (InBounds(x, y)) SetFloor(x, y);
        }

        List<Rectangle> SplitLeaf(Rectangle rect, int depth, Random rng)
        {
            var result = new List<Rectangle>();
            if (depth <= 0 || rect.Width < 8 || rect.Height < 8)
            {
                result.Add(rect);
                return result;
            }

            bool splitVert = rect.Width > rect.Height;

            if (splitVert)
            {
                int minSplit = rect.X + 4;
                int maxSplit = rect.Right - 4;
                if (maxSplit <= minSplit) { result.Add(rect); return result; }

                int split = rng.Next(minSplit, maxSplit);
                var left = new Rectangle(rect.X, rect.Y, split - rect.X, rect.Height);
                var right = new Rectangle(split, rect.Y, rect.Right - split, rect.Height);
                result.AddRange(SplitLeaf(left, depth - 1, rng));
                result.AddRange(SplitLeaf(right, depth - 1, rng));
            }
            else
            {
                int minSplit = rect.Y + 4;
                int maxSplit = rect.Bottom - 4;
                if (maxSplit <= minSplit) { result.Add(rect); return result; }

                int split = rng.Next(minSplit, maxSplit);
                var top = new Rectangle(rect.X, rect.Y, rect.Width, split - rect.Y);
                var bottom = new Rectangle(rect.X, split, rect.Width, rect.Bottom - split);
                result.AddRange(SplitLeaf(top, depth - 1, rng));
                result.AddRange(SplitLeaf(bottom, depth - 1, rng));
            }

            return result;
        }
        public void PlaceRandomTraps(Random rng, int count)
        {
            var candidates = new List<Point>();

            for (int y = 1; y < Height - 1; y++)
                for (int x = 1; x < Width - 1; x++)
                {
                    var p = new Point(x, y);
                    if (!IsFloor(p)) continue;
                    if (Doors.ContainsKey(p)) continue;
                    if (ItemsAt.ContainsKey(p)) continue;
                    if (Traps.ContainsKey(p)) continue;

                    candidates.Add(p);
                }

            // Shuffle and pick a subset
            Shuffler.Shuffle(candidates.ToArray(), rng);

            int placed = 0;
            foreach (var p in candidates)
            {
                if (placed >= count) break;

                Trap trap;
                int roll = rng.Next(100);
                if (roll < 60)
                    trap = new SpikeTrap(p);
                else
                    trap = new ArrowTrap(p, new Point(1, 0)); // for example, east-facing

                Traps[p] = trap;
                placed++;
            }
        }

        void UpdateWalls()
        {
            // Currently a no-op: "wall" is simply "not floor".
            // You can add extra edge-processing here later (e.g. flags for wall tiles).
        }

        // ---------------------------------------------------------------------
        // Doors
        // ---------------------------------------------------------------------
        public List<Point> FindDoorCandidates()
        {
            var candidates = new List<Point>();

            for (int y = 1; y < Height - 1; y++)
                for (int x = 1; x < Width - 1; x++)
                {
                    var p = new Point(x, y);
                    if (!IsFloor(p)) continue;           // door sits on floor
                    if (Doors.ContainsKey(p)) continue;  // no double door

                    bool n = IsFloor(new Point(x, y - 1));
                    bool s = IsFloor(new Point(x, y + 1));
                    bool w = IsFloor(new Point(x - 1, y));
                    bool e = IsFloor(new Point(x + 1, y));

                    bool verticalPassage = n && s && !w && !e;
                    bool horizontalPassage = w && e && !n && !s;

                    if (verticalPassage || horizontalPassage)
                        candidates.Add(p);
                }

            return candidates;
        }

        public void PlaceDoors(Random rng, int maxDoors, int secretChancePercent = 15)
        {
            var candidates = FindDoorCandidates();
            if (candidates.Count == 0 || maxDoors <= 0) return;

            Shuffler.Shuffle(candidates.ToArray(), rng);

            int placed = 0;
            foreach (var p in candidates)
            {
                if (placed >= maxDoors) break;
                if (Doors.ContainsKey(p)) continue;

                if (rng.Next(100) < secretChancePercent)
                {
                    var secretDoor = new SecretDoorObject(p);
                    AddSecretDoor(secretDoor);
                }
                else
                {
                    var door = new DoorObject(p, DoorState.Closed);
                    AddDoor(door);
                }

                placed++;
            }
        }

        public void AddDoor(DoorObject door)
        {
            var p = door.Pos;

            Doors[p] = door;

            // Underlying tile must be floor
            if (!IsFloor(p))
                SetFloor(p.X, p.Y);

            if (!ItemsAt.TryGetValue(p, out var items))
            {
                items = new List<Item>();
                ItemsAt[p] = items;
            }
            // doors aren't in the item list at the moment, drawing uses Doors
        }

        public void AddSecretDoor(SecretDoorObject door)
        {
            var p = door.Pos;

            Doors[p] = door;

            if (!ItemsAt.TryGetValue(p, out var items))
            {
                items = new List<Item>();
                ItemsAt[p] = items;
            }
        }

        public DoorObject GetDoorAt(Point p)
        {
            return Doors.TryGetValue(p, out var d) ? d : null;
        }

        // ---------------------------------------------------------------------
        // Accessors & LOS
        // ---------------------------------------------------------------------
        bool InBounds(int x, int y, int margin = 0)
            => x >= margin && y >= margin && x < Width - margin && y < Height - margin;

        public bool InBounds(Point p)
            => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

        public bool IsFloor(Point p)
            => InBounds(p) && _cells[p.X, p.Y].IsFloor;

        public bool IsPassable(Point p)
        {
            if (!InBounds(p)) return false;

            // Doors override base floor passability
            if (Doors.TryGetValue(p, out var d))
            {
                // Hidden secret door = solid wall
                if (d is SecretDoorObject s && !s.Discovered)
                    return false;

                if (!d.IsWalkable)
                    return false;

                // open door: passable only if the underlying tile is floor
                return IsFloor(p);
            }

            // Normal tiles
            return IsFloor(p);
        }

        public bool IsOpaque(Point p)
        {
            if (!InBounds(p)) return true;

            // Doors
            if (Doors.TryGetValue(p, out var d))
            {
                if (d is SecretDoorObject s && !s.Discovered)
                    return true; // hidden secret door = wall

                // Visible doors:
                // - closed/locked → opaque
                // - open → transparent if floor
                return !IsFloor(p) || !d.IsWalkable;
            }

            // Normal tiles: walls opaque, floors transparent
            return !IsFloor(p);
        }

        public bool HasLineOfSight(Point a, Point b)
        {
            int x0 = a.X, y0 = a.Y, x1 = b.X, y1 = b.Y;
            int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int x = x0, y = y0;
            int px = x, py = y;

            while (true)
            {
                if (!(x == x0 && y == y0))
                {
                    if (x != px && y != py)
                    {
                        var t1 = new Point(x, py);
                        var t2 = new Point(px, y);
                        if (IsOpaque(t1) && IsOpaque(t2)) return false;
                    }

                    var p = new Point(x, y);
                    if (IsOpaque(p))
                        return p == b && !IsOpaque(b);
                }

                if (x == x1 && y == y1) break;

                int e2 = 2 * err;
                px = x; py = y;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }

            return true;
        }

        // ---------------------------------------------------------------------
        // Random positions
        // ---------------------------------------------------------------------
        public Point RandomFloor(Random rng, int minDistance = 0)
        {
            var all = new List<Point>();
            for (int y = 1; y < Height - 1; y++)
                for (int x = 1; x < Width - 1; x++)
                    if (_cells[x, y].IsFloor)
                        all.Add(new Point(x, y));

            if (all.Count == 0) return new Point(1, 1);

            if (minDistance <= 0)
                return all[rng.Next(all.Count)];

            // If you really want minDistance constraint, you can filter here.
            // For now we just ignore minDistance beyond storing param.
            return all[rng.Next(all.Count)];
        }

        public Point RandomFloorNotOccupied(Random rng, Point avoid, List<NonPlayerCharacter> npcs)
        {
            var all = new List<Point>();
            for (int y = 1; y < Height - 1; y++)
                for (int x = 1; x < Width - 1; x++)
                {
                    if (!_cells[x, y].IsFloor) continue;
                    var p = new Point(x, y);
                    if (p == avoid) continue;
                    if (npcs.Exists(n => n.Pos == p)) continue;
                    all.Add(p);
                }
            return all.Count == 0 ? new Point(1, 1) : all[rng.Next(all.Count)];
        }

    }
}
