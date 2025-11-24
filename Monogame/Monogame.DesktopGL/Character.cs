using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeMonoGame;
using RogueTest;

public abstract partial class Character : GameObject, IEmitsSpectra, IVision
{
    public FacingDir FacingDirection = FacingDir.Down;
    public AnimState AnimationState = AnimState.Idle;

    public Dictionary<(AnimState, FacingDir), AnimatedSprite> Animations
        = new Dictionary<(AnimState, FacingDir), AnimatedSprite>();
    // ===== Basic stats =====
    public int MaxHP { get; set; } = 10;
    public int HP { get; set; } = 10;
    public bool IsDead => HP <= 0;
    public int SizeW { get; protected set; } = 1;
    public int SizeH { get; protected set; } = 1;
    public Rectangle Bounds => new Rectangle(Pos.X, Pos.Y, SizeW, SizeH);


    // What this character emits in each spectrum (Light, Scent, Heat, etc.)
    public SpectrumVector Emission { get; } = new SpectrumVector();

    // ===== Vision & inventory =====
    // Innate vision (eg. Elf darkvision, monster omnivision, etc.)
    public readonly List<VisionSource> InnateVision = new();

    public List<Item> Inventory { get; set; } = new();

    // Direction the character is facing – used for cone vision
    public Point Facing { get; set; } = new Point(1, 0);

    // Visible/explored state (visual FOV)
    public bool[,] Visible;   // recomputed every turn
    public bool[,] Explored;  // revealed at least once
    public bool[,] Memory;    // optional: for remembered things

    // Detected creatures this turn via non-visual spectra
    // Key: target Id (eg. NonPlayerCharacter.Id)
    // Value: which spectrum detected them and at what range
    public Dictionary<Guid, (SenseSpectrum spectrum, int range)> DetectedCreatures = new();

    // ===== Constructors =====
    protected Character(Point pos)
    {
        Pos = pos;
    }

    protected Character(Point pos, int hp)
    {
        Pos = pos;
        HP = MaxHP = hp;
    }
    public bool Occupies(Point p)
    {
        return p.X >= Pos.X && p.X < Pos.X + SizeW &&
               p.Y >= Pos.Y && p.Y < Pos.Y + SizeH;
    }
    // ===== HP logic =====
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;
        HP -= amount;
        if (HP < 0) HP = 0;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;
        HP += amount;
        if (HP > MaxHP) HP = MaxHP;
    }

    // ===== Facing helpers =====
    public void SetFacing(Point dir)
    {
        if (dir.X == 0 && dir.Y == 0) return;
        Facing = new Point(Math.Sign(dir.X), Math.Sign(dir.Y));
    }

    public static float DegreesFromDir(Point dir)
    {
        int dx = dir.X, dy = dir.Y;
        if (dx == 0 && dy == 0) return 0f;
        float ang = MathF.Atan2(dy, dx) * (180f / MathF.PI);
        if (ang < 0) ang += 360f;
        return ang;
    }

    // ===== IVision =====
    public virtual IEnumerable<VisionSource> GetVisionSources()
    {
        // 1) Innate
        foreach (var v in InnateVision)
            yield return v;

        // 2) Equipped items that provide IVision
        foreach (var it in GetEquippedItems())
        {
            if (it is IVision vi)
            {
                foreach (var src in vi.GetVisionSources())
                    yield return src;
            }
        }
    }

    // ===== Composite sensing (visual + other spectra) =====
    public void RecomputeSensors(
        DungeonMap map,
        int w,
        int h,
        IEnumerable<NonPlayerCharacter> others,
        IEnumerable<Item> items,
        float coneFacingDeg)
    {
        // Init/resize buffers
        if (Visible == null || Visible.GetLength(0) != w || Visible.GetLength(1) != h)
        {
            Visible = new bool[w, h];
            Explored = new bool[w, h];
        }
        if (Memory == null || Memory.GetLength(0) != w || Memory.GetLength(1) != h)
            Memory = new bool[w, h];

        Array.Clear(Visible, 0, Visible.Length);
        DetectedCreatures.Clear();

        // Gather all vision sources and let Cone sources use latest facing for center
        var sources = new List<VisionSource>();
        foreach (var s in GetVisionSources())
        {
            var c = new VisionSource
            {
                Mode = s.Mode,
                Radius = s.Radius,
                ConeCenterDeg = s.Mode == VisionMode.Cone ? coneFacingDeg : s.ConeCenterDeg,
                ConeHalfWidthDeg = s.ConeHalfWidthDeg,
                Detector = s.Detector.Clone()
            };
            sources.Add(c);
        }

        // === 1) Visual (Light) composite → fills Visible / Explored / Memory ===
        foreach (var src in sources)
        {
            if (src.Detector[SenseSpectrum.Light] <= 0) continue;
            CastFov(map, w, h, src, SenseSpectrum.Light, markVisible: true);
        }

        // Safety ring around self for adjacent clarity (visual)
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                int vx = Pos.X + ox, vy = Pos.Y + oy;
                if (vx < 0 || vy < 0 || vx >= w || vy >= h) continue;
                Visible[vx, vy] = true;
                Explored[vx, vy] = true;
                Memory[vx, vy] = true;
            }
        }
        if (map.InBounds(Pos))
        {
            Visible[Pos.X, Pos.Y] = true;
            Explored[Pos.X, Pos.Y] = true;
            Memory[Pos.X, Pos.Y] = true;
        }

        // === 2) Non-visual spectra → update DetectedCreatures ===
        // Detection condition (simplified):
        //   manhattan(self, target) <= min(source.Radius, detectorStrength + targetEmission)
        //   + we require line-of-effect for now (can relax for smells/noise later)
        void ConsiderTarget(Guid id, Point tp, SpectrumVector targetEmission)
        {
            foreach (var src in sources)
            {
                foreach (SenseSpectrum s in Enum.GetValues(typeof(SenseSpectrum)))
                {
                    int detect = src.Detector[s];
                    if (detect <= 0) continue;

                    int emit = targetEmission[s];
                    int maxRange = detect + emit;
                    if (maxRange <= 0) continue;

                    int dx = Math.Abs(tp.X - Pos.X);
                    int dy = Math.Abs(tp.Y - Pos.Y);
                    int manhattan = dx + dy;
                    if (manhattan == 0)
                    {
                        DetectedCreatures[id] = (s, 0);
                        continue;
                    }

                    int geomLimit = Math.Min(src.Radius, maxRange);
                    if (manhattan > geomLimit) continue;

                    // For now we require line-of-effect for all spectra
                    if (!HasLineOfEffect(map, Pos, tp)) continue;

                    // Keep the closest/strongest detection
                    if (!DetectedCreatures.TryGetValue(id, out var cur) || manhattan < cur.range)
                        DetectedCreatures[id] = (s, manhattan);
                }
            }
        }

        // Detect other NPCs
        foreach (var npc in others)
        {
            if (npc.IsDead) continue;
            ConsiderTarget(npc.Id, npc.Pos, npc.Emission);
        }

        // Items detection via spectra is possible, but you currently don’t use it
        // in the rest of the code, so we skip items here to keep it simple.
    }

    // ===== FOV core =====
    void CastFov(DungeonMap map, int w, int h, VisionSource src, SenseSpectrum s, bool markVisible)
    {
        // Omni = see everything
        if (src.Mode == VisionMode.Omni)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Visible[x, y] = true;
                    Explored[x, y] = true;
                    Memory[x, y] = true;
                }
            }
            return;
        }

        int R = src.Radius;
        int xmin = Math.Max(0, Pos.X - R), xmax = Math.Min(w - 1, Pos.X + R);
        int ymin = Math.Max(0, Pos.Y - R), ymax = Math.Min(h - 1, Pos.Y + R);

        for (int y = ymin; y <= ymax; y++)
        {
            for (int x = xmin; x <= xmax; x++)
            {
                int dx = x - Pos.X;
                int dy = y - Pos.Y;
                int d2 = dx * dx + dy * dy;
                if (d2 > R * R) continue;

                // Cone shaping if needed
                if (src.Mode == VisionMode.Cone)
                {
                    float ang = MathF.Atan2(dy, dx) * (180f / MathF.PI);
                    if (ang < 0) ang += 360f;
                    float diff = MathF.Abs(ang - src.ConeCenterDeg);
                    if (diff > 180f) diff = 360f - diff;
                    if (diff > src.ConeHalfWidthDeg) continue;
                }

                var p = new Point(x, y);
                if (!HasLineOfEffect(map, Pos, p)) continue;

                if (markVisible)
                {
                    Visible[x, y] = true;
                    Explored[x, y] = true;
                    Memory[x, y] = true;
                }
            }
        }
    }
    protected virtual IEnumerable<Item> GetEquippedItems() => Array.Empty<Item>();
    // ===== Line-of-effect / line-of-sight =====
    // IMPORTANT FIX:
    // We now test "are we at the destination?" BEFORE checking opacity.
    // This means opaque tiles (walls/doors) themselves can be seen,
    // but they stop vision *past* them.
    bool HasLineOfEffect(DungeonMap map, Point a, Point b)
    {
        int x0 = a.X, y0 = a.Y, x1 = b.X, y1 = b.Y;
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;
        int x = x0, y = y0;
        bool first = true;

        while (true)
        {
            // Destination reached: do NOT test it for opacity here
            if (x == x1 && y == y1) break;

            // Skip the starting tile, but if any *intermediate* tile is opaque, block
            if (!first && map.IsOpaque(new Point(x, y)))
                return false;

            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x += sx; }
            if (e2 <= dx) { err += dx; y += sy; }
            first = false;
        }

        return true;
    }
    public void UpdateAnimation(float dt)
    {
        if (Animations.TryGetValue((AnimationState, FacingDirection), out var anim))
            anim.Update(dt);
    }

    public void DrawAnimated(SpriteBatch sb, int tileSize, Color color, int ox)
    {
        if (!Animations.TryGetValue((AnimationState, FacingDirection), out var anim))
            return;

        var dest = new Rectangle(
            ox + Pos.X * tileSize,
            Pos.Y * tileSize,
            tileSize, tileSize);

        anim.Draw(sb, dest, color);
    }

}
