using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class CorridorInfo
    {
        public List<Point> Path;   // ordered cells
        public FloorType Floor;
        public WallType Wall;
        public bool IsBridge;      // bridge across wall area
    }

}
