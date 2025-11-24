using Microsoft.Xna.Framework;
using System;

namespace RoguelikeMonoGame
{
    public sealed class HouseInteriorAlgorithm : IDungeonAlgorithm
    {
        public string Name => "HouseInterior";

        public void Generate(bool[,] grid, Random rng, IDungeonConfig cfgObj)
        {
            var cfg = (HouseConfig)cfgObj;
            int W = grid.GetLength(0), H = grid.GetLength(1);

            // Start as all walls
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    grid[x, y] = false;

            // Simple box-room with a couple internal walls
            for (int y = 1; y < H - 1; y++)
                for (int x = 1; x < W - 1; x++)
                    grid[x, y] = true;

            // Add one internal dividing wall if space allows
            if (W > 8 && H > 8)
            {
                int splitX = W / 2;
                for (int y = 2; y < H - 2; y++)
                    grid[splitX, y] = false;
                // door in that wall
                int doorY = rng.Next(3, H - 3);
                grid[splitX, doorY] = true;
            }

            var lvl = new Level
            {
                Id = $"{cfg.Building.Id}-floor-{cfg.FloorIndex}",
                Walkable = grid,
                PlayerStart = new Point(W / 2, H - 2)  // near bottom
            };

            // stairs from below
            if (cfg.StairsFromBelow != null)
            {
                foreach (var up in cfg.StairsFromBelow)
                {
                    grid[up.X, up.Y] = true;
                    // Later, you can spawn a stair object there
                }
            }

            // stairs to above (except top floor)
            if (cfg.FloorIndex < cfg.Building.Floors - 1)
            {
                Point stair;
                do
                {
                    stair = new Point(rng.Next(2, W - 2), rng.Next(2, H - 2));
                } while (!grid[stair.X, stair.Y]);

                cfg.StairsToAbove.Add(stair);
                lvl.StairsDown = stair; // from this floor up to next
            }

            cfg.OutputLevel = lvl;
        }
    }
}
