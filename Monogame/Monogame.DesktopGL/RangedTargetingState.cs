using Microsoft.Xna.Framework;
using RoguelikeMonoGame;

namespace RogueTest
{
    sealed class RangedTargetingState
    {
        public bool Active;
        public RangedShapeKind Shape;
        public int Range;
        public int Radius;           // only used for Circle, Cone, Line length
        public Point Origin;         // usually player position
        public Point TargetCell;     // for cell/circle targeting
        public Point Direction;      // for cone/line targeting
        public int Damage;           // you can route this from the weapon
        public WeaponItem? Weapon;   // optional reference to equipped weapon
        public string UseId = "";    // e.g. "Shoot", "Fireball", etc.
    }
}
