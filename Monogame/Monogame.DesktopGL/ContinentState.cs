namespace RoguelikeMonoGame
{
    public sealed class ContinentState
    {
        public Continent Continent;
        public Region CurrentRegion;
        public Level CurrentLevel;

        public ContinentState(Continent continent, Region region, Level level)
        {
            Continent = continent;
            CurrentRegion = region;
            CurrentLevel = level;
        }
    }
}
