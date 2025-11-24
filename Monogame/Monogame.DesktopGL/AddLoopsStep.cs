using System;

namespace RoguelikeMonoGame
{
    public sealed class AddLoopsStep : IGenStep
    {
        public string Name => "Add Loops";
        private readonly int _percent; // 0..100
        public AddLoopsStep(int percent) { _percent = Math.Clamp(percent, 0, 100); }
        public void Run(bool[,] grid, Random rng)
        {
            int W = grid.GetLength(0), H = grid.GetLength(1);
            int budget = (W * H * _percent) / 800; // modest
            while (budget-- > 0)
            {
                int x = rng.Next(2, W - 2), y = rng.Next(2, H - 2);
                // carve a small 2-3 tile patch to introduce cycles
                for (int i = 0; i < rng.Next(2, 4); i++)
                {
                    int dx = rng.Next(-1, 2), dy = rng.Next(-1, 2);
                    int nx = Math.Clamp(x + dx, 1, W - 2);
                    int ny = Math.Clamp(y + dy, 1, H - 2);
                    grid[nx, ny] = true; x = nx; y = ny;
                }
            }
        }

    }

}
