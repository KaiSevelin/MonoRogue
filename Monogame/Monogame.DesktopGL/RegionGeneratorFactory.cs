using RoguelikeMonoGame;

namespace RogueTest
{
    public static class RegionGeneratorFactory
    {
        public static IRegionGenerator GetGenerator(TerrainType t, int regionWidth, int regionHeight)
        {
            // Build a config based on terrain type.
            var cfg = new OutdoorConfig(regionWidth, regionHeight);

            switch (t)
            {
                case TerrainType.Grassland:
                    cfg.HasHouses = true;
                    cfg.VegetationDensity = 0.4f;
                    cfg.SeaCoverage = 0.05f;
                    cfg.DungeonPortals = 1;
                    break;

                case TerrainType.Forest:
                    cfg.HasHouses = false;
                    cfg.VegetationDensity = 0.8f;
                    cfg.SeaCoverage = 0.02f;
                    cfg.DungeonPortals = 2;
                    break;

                case TerrainType.Mountains:
                    cfg.HasHouses = false;
                    cfg.VegetationDensity = 0.2f;
                    cfg.SeaCoverage = 0.0f;
                    cfg.DungeonPortals = 3;
                    break;

                default:
                    cfg.HasHouses = true;
                    cfg.VegetationDensity = 0.5f;
                    cfg.SeaCoverage = 0.05f;
                    cfg.DungeonPortals = 1;
                    break;
            }

            return new OutdoorGenerator(cfg);
        }
    }
}
