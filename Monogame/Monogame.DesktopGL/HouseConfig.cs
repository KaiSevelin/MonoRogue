using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class HouseConfig : IDungeonConfig
    {
        public BuildingSpec Building;
        public int FloorIndex;                 // 0 = ground, 1, 2...
        public List<Point> StairsFromBelow = new(); // local coords
        public List<Point> StairsToAbove = new();   // filled by generator
        public Level OutputLevel;              // filled by generator
    }

}
