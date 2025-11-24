using Microsoft.Xna.Framework;
using RogueTest;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class Chest : Item, IHiddenRevealable
    {
        public bool IsLocked { get; private set; }
        public bool IsOpen { get; private set; }

        // Chests are “containers” in UI terms
        public override string Slot => "Container";

        // Use a different glyph when open/closed
        public override string Glyph => IsOpen ? "c" : "C";

        // You can choose if the player can stand on an open chest.
        public override bool IsWalkable => IsOpen;

        // Use Item’s IsHidden, but keep it writable here

        public List<Item> Contents { get; } = new();

        public Chest(Point pos, bool locked, IEnumerable<Item>? contents = null)
            : base(pos, locked ? "Locked chest" : "Chest")
        {
            IsLocked = locked;
            if (contents != null)
                Contents.AddRange(contents);
        }

        public bool HasLoot => Contents.Count > 0;

        public bool Discovered { get; set; }

        public override void Interact(
            PlayerCharacter player,
            DungeonMap map,
            List<NonPlayerCharacter> npcs,
            Dictionary<Point, List<Item>> itemsAt)
        {
            // --- First interaction: handle locked / opening ---

            if (!IsOpen)
            {
                if (IsLocked)
                {
                    if (player.Keys > 0)
                    {
                        player.Keys--;
                        IsLocked = false;
                        // optional: message "You unlock the chest."
                    }
                    else
                    {
                        // optional: message "The chest is locked."
                        return;
                    }
                }

                // Now we can open the chest
                IsOpen = true;
                IsHidden = false;

                // Dump contents to the floor as normal items
                if (Contents.Count > 0)
                {
                    if (!itemsAt.TryGetValue(Pos, out var pile))
                    {
                        pile = new List<Item>();
                        itemsAt[Pos] = pile;
                    }
                    pile.AddRange(Contents);
                    Contents.Clear();
                }

                // Do NOT call base.Interact – we do not want to pick up the chest
                return;
            }

            // --- Chest already open: allow the player to loot items on this tile ---

            if (itemsAt.TryGetValue(Pos, out var itemsHere) && itemsHere.Count > 0)
            {
                // For now, just auto-pick the top item; you can later plug in your
                // “pile dialog” here instead.
                var top = itemsHere[^1];
                top.Interact(player, map, npcs, itemsAt);
            }
        }

        // IHiddenRevealable implementation
        public void Reveal(DungeonMap map, PlayerCharacter searcher)
        {
            IsHidden = false;
            // optional: log/message “You notice a hidden chest!”
        }
    }
}
