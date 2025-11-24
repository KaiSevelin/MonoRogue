using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class CityConfig : IDungeonConfig
    {
        public int StreetWidth = 3;

        // smaller houses
        public int BlockMin = 6;
        public int BlockMax = 10;

        public List<BuildingSpec> Buildings = new();
        public List<Level> OutputLevels = new();  // city + house floors
        public Level CityLevel;
    }


}
