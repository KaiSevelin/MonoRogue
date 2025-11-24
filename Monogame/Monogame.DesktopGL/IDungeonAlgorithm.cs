using System;

namespace RoguelikeMonoGame
{
    public interface IDungeonAlgorithm
        {
            string Name { get; }
            void Generate(bool[,] grid, Random rng, IDungeonConfig config);
        }

}
