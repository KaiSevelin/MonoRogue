using System;

namespace RoguelikeMonoGame
{
    public interface IGenStep
    {
        string Name { get; }
        void Run(bool[,] grid, Random rng);
    }

}
