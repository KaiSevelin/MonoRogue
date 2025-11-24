using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class BSPRoomsAlgorithm : IDungeonAlgorithm
    {
        public string Name => "BSP Rooms";
        struct Leaf { public int X, Y, W, H; public Leaf(int x, int y, int w, int h) { X = x; Y = y; W = w; H = h; } }

        public void Generate(bool[,] grid, Random rng, IDungeonConfig cfgObj)
        {
            var cfg = cfgObj as BspRoomsConfig ?? new BspRoomsConfig();
            int w = grid.GetLength(0), h = grid.GetLength(1);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) grid[x, y] = false;

            var leaves = new List<Leaf> { new Leaf(1, 1, w - 2, h - 2) };
            bool didSplit = true; int guard = 0;

            while (didSplit && guard++ < 4096)
            {
                didSplit = false;
                for (int i = 0; i < leaves.Count; i++)
                {
                    var L = leaves[i];
                    bool canH = L.H >= cfg.MinLeafSize * 2;
                    bool canV = L.W >= cfg.MinLeafSize * 2;
                    if (!canH && !canV) continue;

                    bool must = L.W > cfg.MaxLeafSize || L.H > cfg.MaxLeafSize;
                    bool hsplit;
                    if (!must)
                    {
                        if (L.W / (float)L.H >= 1.25f && canV) hsplit = false;
                        else if (L.H / (float)L.W >= 1.25f && canH) hsplit = true;
                        else hsplit = rng.Next(2) == 0;
                    }
                    else hsplit = L.W < L.H ? true : false;

                    if (hsplit && !canH) hsplit = false;
                    if (!hsplit && !canV) hsplit = true;

                    if (hsplit)
                    {
                        int max = L.H - cfg.MinLeafSize; if (max < cfg.MinLeafSize) continue;
                        int split = rng.Next(cfg.MinLeafSize, max + 1);
                        var A = new Leaf(L.X, L.Y, L.W, split);
                        var B = new Leaf(L.X, L.Y + split, L.W, L.H - split);
                        leaves[i] = A; leaves.Insert(i + 1, B);
                        didSplit = true; break;
                    }
                    else
                    {
                        int max = L.W - cfg.MinLeafSize; if (max < cfg.MinLeafSize) continue;
                        int split = rng.Next(cfg.MinLeafSize, max + 1);
                        var A = new Leaf(L.X, L.Y, split, L.H);
                        var B = new Leaf(L.X + split, L.Y, L.W - split, L.H);
                        leaves[i] = A; leaves.Insert(i + 1, B);
                        didSplit = true; break;
                    }
                }
            }

            var rooms = new List<Rectangle>();
            foreach (var L in leaves)
            {
                int availW = Math.Max(0, L.W), availH = Math.Max(0, L.H);
                if (availW < cfg.MinRoomSize || availH < cfg.MinRoomSize) continue;

                int rw = rng.Next(cfg.MinRoomSize, availW + 1);
                int rh = rng.Next(cfg.MinRoomSize, availH + 1);
                int rx = rng.Next(L.X, L.X + availW - rw + 1);
                int ry = rng.Next(L.Y, L.Y + availH - rh + 1);

                var r = new Rectangle(rx, ry, rw, rh);
                rooms.Add(r);
                for (int y = r.Top; y < r.Bottom; y++)
                    for (int x = r.Left; x < r.Right; x++)
                        grid[x, y] = true;
            }

            // connect centers
            if (rooms.Count > 0)
            {
                var centers = new List<Point>();
                foreach (var r in rooms) centers.Add(new Point(r.X + r.Width / 2, r.Y + r.Height / 2));

                var connected = new List<Point> { centers[0] };
                centers.RemoveAt(0);
                while (centers.Count > 0)
                {
                    int bi = 0, bj = 0, best = int.MaxValue;
                    for (int i = 0; i < connected.Count; i++)
                        for (int j = 0; j < centers.Count; j++)
                        {
                            int d = Math.Abs(connected[i].X - centers[j].X) + Math.Abs(connected[i].Y - centers[j].Y);
                            if (d < best) { best = d; bi = i; bj = j; }
                        }
                    var a = connected[bi]; var b = centers[bj];
                    CarveCorridor(grid, a, b, rng);
                    connected.Add(b); centers.RemoveAt(bj);
                }
            }

            // borders
            for (int x = 0; x < w; x++) { grid[x, 0] = false; grid[x, h - 1] = false; }
            for (int y = 0; y < h; y++) { grid[0, y] = false; grid[w - 1, y] = false; }

            static void CarveCorridor(bool[,] grid, Point a, Point b, Random rng)
            {
                bool hfirst = rng.Next(2) == 0;
                if (hfirst) { LineX(grid, a.X, b.X, a.Y); LineY(grid, a.Y, b.Y, b.X); }
                else { LineY(grid, a.Y, b.Y, a.X); LineX(grid, a.X, b.X, b.Y); }
            }
            static void LineX(bool[,] grid, int x0, int x1, int y)
            {
                if (x0 > x1) (x0, x1) = (x1, x0);
                for (int x = Math.Max(0, x0); x <= Math.Min(grid.GetLength(0) - 1, x1); x++) grid[x, y] = true;
            }
            static void LineY(bool[,] grid, int y0, int y1, int x)
            {
                if (y0 > y1) (y0, y1) = (y1, y0);
                for (int y = Math.Max(0, y0); y <= Math.Min(grid.GetLength(1) - 1, y1); y++) grid[x, y] = true;
            }
        }
    }

}
