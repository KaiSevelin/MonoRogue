namespace RoguelikeMonoGame
{
    // ---------- MAZE ----------
    public sealed class MazeConfig : IDungeonConfig
    {
        public enum Carver { DFS, Prim }
        public Carver Mode { get; set; } = Carver.DFS;
        public bool AddRooms { get; set; } = true;
        public RangeInt RoomW { get; set; } = new(4, 8);
        public RangeInt RoomH { get; set; } = new(3, 6);
        public int RoomCount { get; set; } = 6;
        public int BraidPct { get; set; } = 0; // remove dead-ends
    }

}
