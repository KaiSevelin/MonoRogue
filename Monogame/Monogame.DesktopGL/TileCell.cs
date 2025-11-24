namespace RoguelikeMonoGame
{
    public struct TileCell
    {
        public GroundType Ground;     // e.g. Grass, Dirt, Water, Stone
        public WallType? Wall;        // e.g. Brick, Cliff, etc., or null if none

        // Optional: variation / autotile mask ids
        public byte GroundVariant;    // 0 = base, others = edge/transition
        public byte WallVariant;

        // Optional: animated tile type
        public byte AnimationId;      // 0 = none, >0 index into AnimatedTile table
    }

}
