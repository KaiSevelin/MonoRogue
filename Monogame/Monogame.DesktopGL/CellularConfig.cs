namespace RoguelikeMonoGame
{
    // ---------- CELLULAR ----------
    public sealed class CellularConfig : IDungeonConfig
    {
        public double InitialWallChance { get; set; } = 0.48;
        public int Steps { get; set; } = 6;
        public int BirthLimit { get; set; } = 4;
        public int SurvivalLimit { get; set; } = 5;
        public bool Moore { get; set; } = true;

        // NEW: how often to “thicken” 1-tile corridors
        public double WideCorridorChance { get; set; } = 0.2;

        // NEW: minimum width when we widen (2 = make 2-wide, 3 = sometimes 3-wide)
        public int WideCorridorMinWidth { get; set; } = 2;
    }


}
