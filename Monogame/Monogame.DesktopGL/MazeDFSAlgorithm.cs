using System;

namespace RoguelikeMonoGame
{
    public sealed class MazeDFSAlgorithm : IDungeonAlgorithm
    {
        public string Name => "Maze DFS (+rooms)";
        public void Generate(bool[,] grid, Random rng, IDungeonConfig cfgObj)
        {
            var cfg = cfgObj as MazeConfig ?? new MazeConfig();
            int w = grid.GetLength(0), h = grid.GetLength(1);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) grid[x, y] = false;

            int cw = Math.Max(1, (w - 1) / 2), ch = Math.Max(1, (h - 1) / 2);
            bool[,] visited = new bool[cw, ch];

            void CarveCell(int cx, int cy)
            {
                visited[cx, cy] = true;
                int x = cx * 2 + 1, y = cy * 2 + 1;
                grid[x, y] = true;

                var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
                for (int i = 0; i < dirs.Length; i++)
                {
                    int j = rng.Next(i, dirs.Length);
                    (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
                }
                foreach (var (dx, dy) in dirs)
                {
                    int nx = cx + dx, ny = cy + dy;
                    if ((uint)nx >= (uint)cw || (uint)ny >= (uint)ch) continue;
                    if (visited[nx, ny]) continue;
                    grid[x + dx, y + dy] = true;
                    CarveCell(nx, ny);
                }
            }
            CarveCell(rng.Next(cw), rng.Next(ch));

            if (cfg.AddRooms)
            {
                for (int i = 0; i < cfg.RoomCount; i++)
                {
                    int rw = cfg.RoomW.Next(rng), rh = cfg.RoomH.Next(rng);
                    int rx = rng.Next(1, Math.Max(2, w - rw - 1));
                    int ry = rng.Next(1, Math.Max(2, h - rh - 1));
                    for (int y = ry; y < ry + rh; y++)
                        for (int x = rx; x < rx + rw; x++)
                            grid[x, y] = true;
                }
            }

            // borders
            for (int x = 0; x < w; x++) { grid[x, 0] = false; grid[x, h - 1] = false; }
            for (int y = 0; y < h; y++) { grid[0, y] = false; grid[w - 1, y] = false; }
        }
    }

}
