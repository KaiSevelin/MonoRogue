using Microsoft.Xna.Framework;

namespace RoguelikeMonoGame
{
    public sealed class LevelConnection
    {
        public string FromLevelId;
        public Point FromPos;
        public string ToLevelId;
        public Point ToPos;
        public ConnectionType Type;
    }

}
