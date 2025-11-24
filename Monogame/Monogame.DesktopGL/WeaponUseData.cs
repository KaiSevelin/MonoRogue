using RogueTest;

namespace RoguelikeMonoGame
{
    public sealed class WeaponUseData
    {
        // "Stab","Slash","Throw","Shoot" (extensible)
        public string Type { get; set; } = "Stab";

        // Common params
        public int Damage { get; set; } = 4;
        public int Range { get; set; } = 1;     // tiles, for Throw/Shoot
        public string ProjectileGlyph { get; set; } = "*";
        public bool ConsumesItem { get; set; } = false; // Throw: dagger can be consumed or dropped
        public string AmmoItemId { get; set; } = "";    // Shoot: optional ammo id
        public RangedShapeKind? RangeType { get; set; } = null; // e.g., Cone, Line, etc.
    }

}
