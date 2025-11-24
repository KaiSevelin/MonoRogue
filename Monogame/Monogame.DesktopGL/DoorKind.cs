namespace RoguelikeMonoGame
{
    public enum DoorKind
    {
        Normal,   // open/closed/locked
        Secret,
        Arch,     // always open, passable, not opaque
        Bars      // not passable, but not opaque
    }

}
