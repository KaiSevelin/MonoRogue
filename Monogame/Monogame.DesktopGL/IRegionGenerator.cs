using System;

namespace RoguelikeMonoGame
{
    public interface IRegionGenerator
    {
        void GenerateRegion(Region region, Random rng);
    }

}
