using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    // ---------- FINISHING STEPS ----------
    public sealed class EnsureConnectivityStep : IGenStep
    {

        public string Name => "Ensure Connectivity";
        public void Run(bool[,] grid, Random rng)
        {
            int W = grid.GetLength(0), H = grid.GetLength(1);
            // Simple: pick first floor, flood-fill, keep only connected region
            Point start = new(1, 1);
            bool found = false;
            for (int y = 0; y < H && !found; y++)
                for (int x = 0; x < W && !found; x++)
                    if (grid[x, y]) { start = new Point(x, y); found = true; }

            if (!found) return;

            var q = new Queue<Point>();
            var seen = new bool[W, H];
            q.Enqueue(start); seen[start.X, start.Y] = true;
            var dirs = new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                foreach (var d in dirs)
                {
                    int nx = p.X + d.X, ny = p.Y + d.Y;
                    if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue;
                    if (!grid[nx, ny] || seen[nx, ny]) continue;
                    seen[nx, ny] = true; q.Enqueue(new Point(nx, ny));
                }
            }
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (grid[x, y] && !seen[x, y]) grid[x, y] = false;
        }
    }

}
