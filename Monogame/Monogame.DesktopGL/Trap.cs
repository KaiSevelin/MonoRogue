using Microsoft.Xna.Framework;
using RogueTest;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public abstract class Trap : GameObject, IHiddenRevealable
    {
        public string Name { get; }
        public bool IsArmed { get; protected set; } = true;

        // Hidden until detected
        public bool Discovered { get;  set; } = false;

        // Some traps might emit something detectable (noise, magic, etc.)
        public virtual SpectrumVector Emission => SpectrumVector.Zero;

        protected Trap(Point pos, string name)
        {
            Pos = pos;
            Name = name;
        }

        // Called when someone steps onto the tile
        public abstract void Trigger(Character target, DungeonMap map, List<NonPlayerCharacter> npcs);

        // Called by search/detection (player or NPC)
        public virtual void Reveal()
        {
            Discovered = true;
        }

        // Optional: what happens if the player explicitly interacts with trap
        public override void Interact(PlayerCharacter player, DungeonMap map, List<NonPlayerCharacter> npcs,
                                      Dictionary<Point, List<Item>> itemsAt)
        {
            // Disarm attempt, for example
            if (!Discovered)
            {
                // Maybe you fail without knowledge
                return;
            }

            // simple disarm success chance stub
            IsArmed = false;
        }

        public void Reveal(DungeonMap map, PlayerCharacter searcher)
        {
            if (Discovered) return;
            Discovered = true;

        }
    }
}
