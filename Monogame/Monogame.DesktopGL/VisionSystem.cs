using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RogueTest;

namespace RoguelikeMonoGame
{
    /// <summary>
    /// Orchestrates vision + light for the player.
    /// Uses Character.RecomputeSensors internally.
    /// </summary>
    public sealed class VisionSystem
    {
        public int Width { get; }
        public int Height { get; }

        // Light buffer used only for *visual* lighting in rendering
        // (completely separate from spectral detection)
        public float[,] Light { get; }

        public VisionSystem(int width, int height)
        {
            Width = width;
            Height = height;
            Light = new float[width, height];
        }

        /// <summary>
        /// Recompute all sensing and light for the player.
        /// Call this once each time the world advances (eg. after a player or NPC turn).
        /// </summary>
        public void Recompute(
            DungeonMap map,
            PlayerCharacter player,
            IReadOnlyList<NonPlayerCharacter> npcs,
            Dictionary<Point, List<Item>> itemsAt)
        {
            // Flatten items for sensors (you can later include item emission if you want)
            var allItems = itemsAt.Values.SelectMany(l => l).ToList();

            float facingDeg = Character.DegreesFromDir(player.Facing);

            // 1) Ask the player to recompute all of their sensory data
            player.RecomputeSensors(
                map,
                Width,
                Height,
                npcs,
                allItems,
                facingDeg);

            // 2) Rebuild the light buffer based on the player's *visual* sources
            Array.Clear(Light, 0, Light.Length);

            var visualSources = player
                .GetVisionSources()
                .Where(v => v.Detector[SenseSpectrum.Light] > 0)
                .ToList();

            foreach (var src in visualSources)
            {
                // We only care about approximate brightness for rendering,
                // not spectral detail – simple radial falloff.
                ApplyRadialLight(map, player.Pos, src);
            }

            // Ensure the player’s own tile is lit at least a little
            if (map.InBounds(player.Pos))
            {
                Light[player.Pos.X, player.Pos.Y] = Math.Max(Light[player.Pos.X, player.Pos.Y], 1f);
            }
        }

        void ApplyRadialLight(DungeonMap map, Point origin, VisionSource src)
        {
            int R = src.Radius;
            int xmin = Math.Max(0, origin.X - R);
            int xmax = Math.Min(Width - 1, origin.X + R);
            int ymin = Math.Max(0, origin.Y - R);
            int ymax = Math.Min(Height - 1, origin.Y + R);

            // For a Cone we also use its angle; for Omni/Ambient we skip angle checks.
            bool isCone = src.Mode == VisionMode.Cone;
            float centerDeg = src.ConeCenterDeg;
            float halfDeg = src.ConeHalfWidthDeg;

            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    int dx = x - origin.X;
                    int dy = y - origin.Y;
                    int distSq = dx * dx + dy * dy;
                    if (distSq == 0) { Light[x, y] = Math.Max(Light[x, y], 1f); continue; }

                    float dist = MathF.Sqrt(distSq);
                    if (dist > R) continue;

                    // For cones, check angle
                    if (isCone)
                    {
                        float ang = Character.DegreesFromDir(new Point(dx, dy));
                        float delta = AngleDelta(centerDeg, ang);
                        if (Math.Abs(delta) > halfDeg) continue;
                    }

                    // Occlusion – if there’s no line-of-sight from origin, skip
                    if (!map.HasLineOfSight(origin, new Point(x, y)))
                        continue;

                    // Simple falloff 1 -> 0 by radius
                    float factor = 1f - (dist / (R + 0.001f));
                    if (factor <= 0f) continue;
                    float cur = Light[x, y];
                    if (factor > cur) Light[x, y] = factor;
                }
            }
        }

        static float AngleDelta(float a, float b)
        {
            float d = (b - a + 540f) % 360f - 180f;
            return d;
        }
    }
}
