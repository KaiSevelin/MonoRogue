using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class PrefabPlacementStep : IGenStep
    {
        public string Name => "Prefab Placement";
        private readonly List<PrefabRoom> _prefabs;
        private readonly int _triesPerPrefab;
        public readonly List<Rectangle> Placed = new(); // for connectors/decor steps

        public PrefabPlacementStep(List<PrefabRoom> prefabs, int triesPerPrefab = 80)
        {
            _prefabs = prefabs ?? new();
            _triesPerPrefab = Math.Max(1, triesPerPrefab);
        }

        public void Run(bool[,] grid, Random rng)
        {
            Placed.Clear();
            int W = grid.GetLength(0), H = grid.GetLength(1);
            var occupied = new List<Rectangle>();

            foreach (var pf in _prefabs)
            {
                bool placed = false;
                for (int t = 0; t < _triesPerPrefab && !placed; t++)
                {
                    int x = rng.Next(1, Math.Max(2, W - pf.Width - 1));
                    int y = rng.Next(1, Math.Max(2, H - pf.Height - 1));
                    var rect = new Rectangle(x, y, pf.Width, pf.Height);

                    // No overlap with previous prefabs (+1 border)
                    bool overlaps = false;
                    foreach (var r in occupied)
                    {
                        var grown = new Rectangle(r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2);
                        if (grown.Intersects(rect)) { overlaps = true; break; }
                    }
                    if (overlaps) continue;

                    // Stamp floors from mask
                    for (int yy = 0; yy < pf.Height; yy++)
                        for (int xx = 0; xx < pf.Width; xx++)
                            if (pf.Mask[xx, yy])
                                grid[x + xx, y + yy] = true;

                    Placed.Add(rect);
                    occupied.Add(rect);
                    placed = true;
                }
            }
        }
    }

}
