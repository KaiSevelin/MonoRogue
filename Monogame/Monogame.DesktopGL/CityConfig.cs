using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class CityConfig : IDungeonConfig
    {
        public int StreetWidth = 3;
        public int BlockMin = 12;
        public int BlockMax = 20;

        public List<BuildingSpec> Buildings = new();
        public List<Level> OutputLevels = new();  // city + house floors
        public Level CityLevel;
    }

}
