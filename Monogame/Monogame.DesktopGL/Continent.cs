using Microsoft.Xna.Framework;
using RogueTest;
using System;

namespace RoguelikeMonoGame
{
    public sealed class Continent
    {
        public Region[,] Regions; // e.g. [worldX, worldY]

        public Continent(int w, int h)
        {
            Regions = new Region[w, h];
        }
        public void InitializeRegions(int regionWidth, int regionHeight, Random rng)
        {
            for (int y = 0; y < Regions.GetLength(1); y++)
                for (int x = 0; x < Regions.GetLength(0); x++)
                {
                    var terrain = TerrainType.Grassland; // for now, all grassland
                    var region = new Region($"region-{x}-{y}", terrain, "kingdom-1")
                    {
                        Walkable = new bool[regionWidth, regionHeight]
                    };

                    var gen = RegionGeneratorFactory.GetGenerator(terrain, regionWidth, regionHeight);
                    gen.GenerateRegion(region, rng);

                    Regions[x, y] = region;
                }
        }
    }
}
