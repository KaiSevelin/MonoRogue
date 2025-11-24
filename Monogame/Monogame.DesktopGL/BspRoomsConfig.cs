namespace RoguelikeMonoGame
{
    // ---------- BSP ----------
    public sealed class BspRoomsConfig : IDungeonConfig
    {
        public int MinLeafSize { get; set; } = 8;
        public int MaxLeafSize { get; set; } = 20;
        public int MinRoomSize { get; set; } = 4;
        public int CorridorWiggle { get; set; } = 0; // future use
        public int ExtraConnectorChancePct { get; set; } = 10;
    }

}
