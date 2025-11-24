namespace RoguelikeMonoGame
{
    public interface IHiddenRevealable
    {
        bool Discovered { get; set; }
        void Reveal(DungeonMap map, PlayerCharacter searcher);
    }

}
