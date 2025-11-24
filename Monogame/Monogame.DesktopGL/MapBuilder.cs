using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class MapBuilder
    {
        private readonly int _w, _h;
        private readonly List<IGenStep> _steps = new();
        public MapBuilder(int w, int h) { _w = w; _h = h; }
        public MapBuilder Step(IGenStep s) { _steps.Add(s); return this; }

        public bool[,] Build(Random rng)
        {
            var grid = new bool[_w, _h];
            foreach (var s in _steps) s.Run(grid, rng);
            return grid;
        }
    }

}
