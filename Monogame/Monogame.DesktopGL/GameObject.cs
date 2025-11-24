using Microsoft.Xna.Framework;
using RogueTest;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    // =========================
    // Base object & layers
    // =========================
    public abstract class GameObject
    {
        public Point Pos;
        public virtual bool IsWalkable => true;
        public virtual string Glyph { get; set; } = "?";
        public abstract void Interact(
            PlayerCharacter player,
            DungeonMap map,
            List<NonPlayerCharacter> npcs,
            Dictionary<Point, List<Item>> itemGrid);
    }



}
