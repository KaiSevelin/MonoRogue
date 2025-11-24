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

            // Start all walls (false = wall, true = floor/walkable)
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    grid[x, y] = false;

            // Simple orthogonal main streets
            int midX = W / 2, midY = H / 2;
            for (int x = 0; x < W; x++)
                for (int dy = -cfg.StreetWidth / 2; dy <= cfg.StreetWidth / 2; dy++)
                    if (midY + dy >= 0 && midY + dy < H)
                        grid[x, midY + dy] = true;

            for (int y = 0; y < H; y++)
                for (int dx = -cfg.StreetWidth / 2; dx <= cfg.StreetWidth / 2; dx++)
                    if (midX + dx >= 0 && midX + dx < W)
                        grid[midX + dx, y] = true;

            // Place rectangular buildings off streets
            int idCounter = 0;
            for (int i = 0; i < 12; i++)
            {
                int w = rng.Next(cfg.BlockMin, cfg.BlockMax);
                int h = rng.Next(cfg.BlockMin, cfg.BlockMax);
                int x = rng.Next(1, W - w - 1);
                int y = rng.Next(1, H - h - 1);
                var rect = new Rectangle(x, y, w, h);

                // Avoid overlapping the central cross too much
                if (rect.Contains(midX, midY)) continue;

                // Carve a tiny yard path to street (for entrance)
                int ex = rect.X + rect.Width / 2;
                int ey = rect.Bottom; // below building
                if (ey < H)
                {
                    grid[ex, ey] = true;
                }

                var b = new BuildingSpec
                {
                    Id = $"house-{idCounter++}",
                    Footprint = rect,
                    Floors = rng.Next(1, 4),
                    Entrance = new Point(ex, ey)
                };
                cfg.Buildings.Add(b);

                // Make building footprint non-walkable in city (we enter via separate house level)
                for (int yy = rect.Y; yy < rect.Bottom; yy++)
                    for (int xx = rect.X; xx < rect.Right; xx++)
                        grid[xx, yy] = false;
            }

            // Build a Level for the city
            var lvl = new Level
            {
                Id = "city",
                Walkable = grid,
                PlayerStart = new Point(midX, midY) // start in the central cross
            };

            cfg.CityLevel = lvl;
        }
    }
}
