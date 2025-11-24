namespace RoguelikeMonoGame
{
    // ---------- PREFABS ----------
    public sealed class PrefabRoom
    {
        // 'true' = floor, 'false' = wall; will be stamped with walls around it preserved
        public bool[,] Mask; // width x height
        public PrefabRoom(bool[,] mask) { Mask = mask; }
        public int Width => Mask.GetLength(0);
        public int Height => Mask.GetLength(1);
    }

}
