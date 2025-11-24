namespace RoguelikeMonoGame
{
    // ---------- ROOMS + PILLARS ----------
    public sealed class RoomsPillarsConfig : IDungeonConfig
    {
        public double ScatterWallChance { get; set; } = 0.15;
        public RangeInt RoomW { get; set; } = new(5, 11);
        public RangeInt RoomH { get; set; } = new(4, 8);
        public int RoomCountScale { get; set; } = 400; // larger => fewer rooms
    }

}
