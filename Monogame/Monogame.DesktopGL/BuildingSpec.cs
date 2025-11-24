using Microsoft.Xna.Framework;

namespace RoguelikeMonoGame
{
    public sealed class BuildingSpec
    {
        public string Id;
        public Rectangle Footprint;   // in city grid coords
        public int Floors;
        public Point Entrance;
    }

}
