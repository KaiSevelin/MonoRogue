using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class WallArea
    {
        public HashSet<Point> Cells = new();  // water, lava, chasm, etc.
        public bool Passable => false;
        public bool Opaque => false;          // your spec: “not passable, not opaque”
    }

}
