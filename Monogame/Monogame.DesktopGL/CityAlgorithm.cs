using Microsoft.Xna.Framework;
using System;

namespace RoguelikeMonoGame
{
    public sealed class CityAlgorithm : IDungeonAlgorithm
    {
        public string Name => "City";

        public void Generate(bool[,] grid, Random rng, IDungeonConfig cfgObj)
        {
            var cfg = (CityConfig)cfgObj;
            int W = grid.GetLength(0);
            int H = grid.GetLength(1);

            if (W < 10 || H < 10)
                throw new ArgumentException("CityAlgorithm: map too small.");

            // ---------------------------------------------------------
            // 1) Start as ALL FLOOR (true = walkable)
            // ---------------------------------------------------------
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    grid[x, y] = true;

            // Border walls so you can't walk off-map
            for (int x = 0; x < W; x++)
            {
                grid[x, 0] = false;
                grid[x, H - 1] = false;
            }
            for (int y = 0; y < H; y++)
            {
                grid[0, y] = false;
                grid[W - 1, y] = false;
            }

            // ---------------------------------------------------------
            // 2) Central cross of streets
            // ---------------------------------------------------------
            int midX = W / 2;
            int midY = H / 2;
            int halfStreet = Math.Max(1, cfg.StreetWidth / 2);

            // vertical street
            for (int y = 1; y < H - 1; y++)
            {
                for (int dx = -halfStreet; dx <= halfStreet; dx++)
                {
                    int xx = midX + dx;
                    if (xx <= 0 || xx >= W - 1) continue;
                    grid[xx, y] = true;
                }
            }

            // horizontal street
            for (int x = 1; x < W - 1; x++)
            {
                for (int dy = -halfStreet; dy <= halfStreet; dy++)
                {
                    int yy = midY + dy;
                    if (yy <= 0 || yy >= H - 1) continue;
                    grid[x, yy] = true;
                }
            }

            // Cross centre (player start) is floor for sure
            grid[midX, midY] = true;

            // ---------------------------------------------------------
            // 3) Sprinkle building blocks (solid wall rectangles)
            // ---------------------------------------------------------
            cfg.Buildings.Clear();
            cfg.OutputLevels.Clear();
            cfg.CityLevel = null;

            int attempts = 40;
            int idCounter = 0;

            int vLeft = midX - (halfStreet + 1);
            int vRight = midX + (halfStreet + 1);
            int hTop = midY - (halfStreet + 1);
            int hBottom = midY + (halfStreet + 1);

            for (int i = 0; i < attempts; i++)
            {
                int bw = rng.Next(cfg.BlockMin, cfg.BlockMax + 1);
                int bh = rng.Next(cfg.BlockMin, cfg.BlockMax + 1);

                if (bw >= W - 4 || bh >= H - 4)
                    continue;

                int bx = rng.Next(2, W - bw - 2);
                int by = rng.Next(2, H - bh - 2);
                var rect = new Rectangle(bx, by, bw, bh);

                // don't stamp onto the cross
                bool touchesVertical =
                    rect.Right >= vLeft && rect.Left <= vRight;
                bool touchesHorizontal =
                    rect.Bottom >= hTop && rect.Top <= hBottom;

                if (touchesVertical || touchesHorizontal)
                    continue;

                // don't overlap existing buildings
                bool overlaps = false;
                for (int yy = rect.Top; yy < rect.Bottom && !overlaps; yy++)
                {
                    for (int xx = rect.Left; xx < rect.Right; xx++)
                    {
                        if (!grid[xx, yy])
                        {
                            overlaps = true;
                            break;
                        }
                    }
                }
                if (overlaps) continue;

                // mark building interior as walls (not walkable)
                for (int yy = rect.Top; yy < rect.Bottom; yy++)
                    for (int xx = rect.Left; xx < rect.Right; xx++)
                        grid[xx, yy] = false;

                // choose an entrance just outside one side
                int ex, ey;
                switch (rng.Next(4))
                {
                    case 0: // top
                        ex = rng.Next(rect.Left, rect.Right);
                        ey = rect.Top - 1;
                        break;
                    case 1: // bottom
                        ex = rng.Next(rect.Left, rect.Right);
                        ey = rect.Bottom;
                        break;
                    case 2: // left
                        ex = rect.Left - 1;
                        ey = rng.Next(rect.Top, rect.Bottom);
                        break;
                    default: // right
                        ex = rect.Right;
                        ey = rng.Next(rect.Top, rect.Bottom);
                        break;
                }

                if (ex > 0 && ex < W - 1 && ey > 0 && ey < H - 1)
                    grid[ex, ey] = true; // entrance is walkable

                var b = new BuildingSpec
                {
                    Id = $"house-{idCounter++}",
                    Footprint = rect,
                    Floors = rng.Next(1, 4),
                    Entrance = new Point(ex, ey)
                };
                cfg.Buildings.Add(b);
            }

            // ---------------------------------------------------------
            // 4) Build Level
            // ---------------------------------------------------------
            var lvl = new Level
            {
                Id = "city",
                Walkable = grid,
                PlayerStart = new Point(midX, midY)
            };

            cfg.CityLevel = lvl;
            cfg.OutputLevels.Add(lvl);
        }
    }
}
