using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class TowerConfig : IDungeonConfig
    {
        public string Id;
        public int Floors;
        public int Radius;            // approximate tower radius
        public List<Point> StairsPerFloor = new(); // local coords, repeated across floors
        public List<Level> OutputLevels = new();
    }

}
