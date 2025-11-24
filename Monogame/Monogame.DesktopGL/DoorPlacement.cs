using Microsoft.Xna.Framework;

namespace RoguelikeMonoGame
{
    public sealed class DoorPlacement
    {
        public Point Pos;
        public DoorKind Kind;
        public DoorMaterial Material;
        public bool InitiallyOpen;
        public bool InitiallyLocked;
    }

}
