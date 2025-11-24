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
             
            // Place rectangular buildings off streets
            int idCounter = 0;

            // number of attempts scales with map size
            int attempts = (W * H) / (cfg.BlockMax * cfg.BlockMax / 2);
            if (attempts < 16) attempts = 16;
            if (attempts > 60) attempts = 60;

            bool OverlapsAny(Rectangle r)
            {
                foreach (var b in cfg.Buildings)
                    if (b.Footprint.Intersects(r))
                        return true;
                return false;
            }

            void PlaceBuilding(Rectangle rect)
            {
                // Clamp inside bounds
                if (rect.Left <= 1 || rect.Top <= 1) return;
                if (rect.Right >= W - 1 || rect.Bottom >= H - 1) return;

                if (OverlapsAny(rect)) return;
                if (rect.Contains(midX, midY)) return; // not on the central cross

                // carve a tiny path from entrance to the nearest street
                int ex = rect.X + rect.Width / 2;
                int ey = rect.Bottom;

                if (ey >= H) ey = rect.Y - 1;        // try above if below is out of bounds
                if (ey <= 0 || ey >= H) return;

                grid[ex, ey] = true;                 // path / small yard

                var b = new BuildingSpec
                {
                    Id = $"house-{idCounter++}",
                    Footprint = rect,
                    Floors = rng.Next(1, 4),
                    Entrance = new Point(ex, ey)
                };
                cfg.Buildings.Add(b);

                // footprint is non-walkable (walls) – we enter via interior levels instead
                for (int yy = rect.Y; yy < rect.Bottom; yy++)
                    for (int xx = rect.X; xx < rect.Right; xx++)
                        grid[xx, yy] = false;
            }

            // 4 “guaranteed” houses around the central cross (if they fit)
            Rectangle[] aroundCenter =
            {
    new(midX - 12, midY - 8,  8, 6),
    new(midX + 4,  midY - 8,  8, 6),
    new(midX - 12, midY + 2,  8, 6),
    new(midX + 4,  midY + 2,  8, 6),
};
            foreach (var r in aroundCenter)
                PlaceBuilding(r);

            // plus lots of random houses
            for (int i = 0; i < attempts; i++)
            {
                int w = rng.Next(cfg.BlockMin, cfg.BlockMax);
                int h = rng.Next(cfg.BlockMin, cfg.BlockMax);
                int x = rng.Next(1, W - w - 1);
                int y = rng.Next(1, H - h - 1);
                var rect = new Rectangle(x, y, w, h);
                PlaceBuilding(rect);
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
