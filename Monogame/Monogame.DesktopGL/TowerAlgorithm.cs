using Microsoft.Xna.Framework;
using System;

namespace RoguelikeMonoGame
{
    public sealed class TowerAlgorithm : IDungeonAlgorithm
    {
        public string Name => "Tower";

        public void Generate(bool[,] grid, Random rng, IDungeonConfig cfgObj)
        {
            var cfg = (TowerConfig)cfgObj;
            int W = grid.GetLength(0), H = grid.GetLength(1);
            int cx = W / 2, cy = H / 2;
            int R = cfg.Radius;

            // Cylinder footprint
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int dx = x - cx, dy = y - cy;
                    int d2 = dx * dx + dy * dy;
                    bool inside = d2 <= R * R;
                    grid[x, y] = inside;
                }

            // Room & corridor inside tower
            // e.g. carve ring corridor, central room, etc.

            var lvl = new Level
            {
                Id = cfg.Id,   // for one floor; you'll call Generate once per floor with different Id
                Walkable = grid,

            };
            lvl.Tiles = new TileCell[W, H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    if (grid[x, y])
                        lvl.Tiles[x, y] = new TileCell() {Ground = GroundType.Cobble};
                    else
                        lvl.Tiles[x, y] = new TileCell() { Ground = GroundType.Cobble, Wall=WallType.Brick };
                }

            // Choose a staircase location (consistent across floors)
            if (cfg.StairsPerFloor.Count == 0)
            {
                // pick a walkable cell near the center
                Point stair;
                do
                {
                    stair = new Point(cx + rng.Next(-2, 3), cy + rng.Next(-2, 3));
                } while (!grid[stair.X, stair.Y]);
                cfg.StairsPerFloor.Add(stair);
            }

            // You’ll call this generator per floor; the stair positions are known from cfg.StairsPerFloor[0]

            cfg.OutputLevels.Add(lvl);
        }
    }
}
