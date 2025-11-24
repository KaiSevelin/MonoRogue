
using System;

namespace RoguelikeMonoGame
{
    public sealed class CellularCavesAlgorithm : IDungeonAlgorithm
    {
        public string Name => "Cellular Caves";
        public void Generate(bool[,] grid, Random rng, IDungeonConfig cfgObj)
        {
            var cfg = cfgObj as CellularConfig ?? new CellularConfig();
            int w = grid.GetLength(0), h = grid.GetLength(1);

            bool[,] cur = new bool[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (x == 0 || y == 0 || x == w - 1 || y == h - 1) { cur[x, y] = false; continue; }
                    cur[x, y] = rng.NextDouble() > cfg.InitialWallChance;
                }

            for (int s = 0; s < cfg.Steps; s++)
            {
                bool[,] nxt = new bool[w, h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        if (x == 0 || y == 0 || x == w - 1 || y == h - 1) { nxt[x, y] = false; continue; }
                        int n = CountSolidNeighbors(cur, x, y, cfg.Moore);
                        nxt[x, y] = cur[x, y] ? n >= cfg.SurvivalLimit : n > cfg.BirthLimit;
                    }
                cur = nxt;
            }

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    grid[x, y] = cur[x, y];
            ThickenPassages(grid, rng, cfg);
        }
        static void ThickenPassages(bool[,] grid, Random rng, CellularConfig cfg)
        {
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);

            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    if (!grid[x, y]) continue; // only existing floor

                    bool north = grid[x, y - 1];
                    bool south = grid[x, y + 1];
                    bool west = grid[x - 1, y];
                    bool east = grid[x + 1, y];

                    int vertical = (north ? 1 : 0) + (south ? 1 : 0);
                    int horizontal = (west ? 1 : 0) + (east ? 1 : 0);

                    // crude detection of a 1-tile corridor cell
                    bool verticalCorridor = vertical >= 1 && horizontal == 0;
                    bool horizontalCorridor = horizontal >= 1 && vertical == 0;

                    if (!(verticalCorridor || horizontalCorridor))
                        continue;

                    if (rng.NextDouble() > cfg.WideCorridorChance)
                        continue;

                    if (verticalCorridor)
                    {
                        // widen east/west
                        if (!grid[x - 1, y]) grid[x - 1, y] = true;
                        if (cfg.WideCorridorMinWidth > 2 && !grid[x + 1, y]) grid[x + 1, y] = true;
                    }
                    else if (horizontalCorridor)
                    {
                        // widen north/south
                        if (!grid[x, y - 1]) grid[x, y - 1] = true;
                        if (cfg.WideCorridorMinWidth > 2 && !grid[x, y + 1]) grid[x, y + 1] = true;
                    }
                }
            }
        }

        int CountSolidNeighbors(bool[,] g, int x, int y, bool moore)
        {
            int w = g.GetLength(0), h = g.GetLength(1), c = 0;
            for (int yy = y - 1; yy <= y + 1; yy++)
                for (int xx = x - 1; xx <= x + 1; xx++)
                {
                    if (xx == x && yy == y) continue;
                    if (!moore && (xx != x && yy != y)) continue; // von Neumann
                    if (xx < 0 || yy < 0 || xx >= w || yy >= h) { c++; continue; }
                    if (!g[xx, yy]) c++;
                }
            return c;
        }
    }

}
