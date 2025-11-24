using Microsoft.Xna.Framework;
using RoguelikeMonoGame;
using System.Collections.Generic;

namespace RogueTest
{
    public abstract class Item : GameObject
    {
        public string Name;
        public virtual string Slot => "Misc";
        public override bool IsWalkable => true;
        public virtual SpectrumVector Emission { get; } = SpectrumVector.Zero;
        public virtual bool IsHidden { get; protected set; } = false;
        public override string Glyph { get;  set; } = "•";


        // ❌ REMOVE all of these from Item:
        // bool _advanceTurn;
        // Dictionary<Guid, Point> _npcLastKnown;
        // Dictionary<Guid, Point> _npcLastSeen;
        // HashSet<Guid> _visibleNpcIds;
        // bool _showItemDialog;
        // List<Item>? _dialogItems;

        protected Item(Point pos, string name)
        {
            Pos = pos;
            Name = name;
        }

        protected Item(string name)
        {
            Name = name;
            Pos = new Point(-1, -1); // “in inventory”
        }

        protected Item(Point pos, string name, string glyph)
        {
            Pos = pos;
            Name = name;
            Glyph = glyph;
        }

        public override void Interact(PlayerCharacter player, DungeonMap map,
                                      List<NonPlayerCharacter> npcs,
                                      Dictionary<Point, List<Item>> itemGrid)
        {
            var oldPos = Pos;

            if (itemGrid.TryGetValue(oldPos, out var items))
            {
                if (items.Remove(this))
                {
                    Pos = new Point(-1, -1); // inventory
                    player.Inventory.Add(this);
                    OnPickedUp(player);

                    if (items.Count == 0)
                        itemGrid.Remove(oldPos);
                }
            }
        }


        protected virtual void OnPickedUp(PlayerCharacter player) { }
        public virtual void OnDropped(PlayerCharacter player) { }
        public virtual bool CanEquip => false;
        public virtual void OnEquip(PlayerCharacter player) { }
        public virtual void OnUnequip(PlayerCharacter player) { }
    }
}