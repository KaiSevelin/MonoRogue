using Microsoft.Xna.Framework;
using RogueTest;

namespace RoguelikeMonoGame
{
    public abstract class DataBackedItem : Item
    {
        public string DefId { get; }
        protected DataBackedItem(Point pos, string name, string defId) : base(pos, name) { DefId = defId; }
        protected DataBackedItem(string name, string defId) : base(name) { DefId = defId; }
    }
}
