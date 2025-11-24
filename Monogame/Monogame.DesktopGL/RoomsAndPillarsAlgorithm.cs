using System;

namespace RoguelikeMonoGame
{
    public sealed class RoomsAndPillarsAlgorithm : IDungeonAlgorithm
    {
        public string Name => "Rooms + Pillars";
        public void Generate(bool[,] grid, Random rng, IDungeonConfig cfgObj)
        {
            var cfg = cfgObj as RoomsPillarsConfig ?? new RoomsPillarsConfig();
            int w = grid.GetLength(0), h = grid.GetLength(1);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (x == 0 || y == 0 || x == w - 1 || y == h - 1) { grid[x, y] = false; continue; }
                    grid[x, y] = rng.NextDouble() > cfg.ScatterWallChance;
                }

            int rooms = Math.Max(4, (w * h) / Math.Max(100, cfg.RoomCountScale));
            for (int i = 0; i < rooms; i++)
            {
                int rw = cfg.RoomW.Next(rng);
                int rh = cfg.RoomH.Next(rng);
                int rx = rng.Next(1, Math.Max(2, w - rw - 1));
                int ry = rng.Next(1, Math.Max(2, h - rh - 1));
                for (int y = ry; y < ry + rh; y++)
                    for (int x = rx; x < rx + rw; x++)
                        grid[x, y] = true;
            }
        }
    }

}
