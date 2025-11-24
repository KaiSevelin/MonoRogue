using Microsoft.Xna.Framework;
using RogueTest;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public class DoorObject : Item
    {
        public bool IsOpen => State == DoorState.Open;
        public bool IsLocked => State == DoorState.Locked;

        public DoorState State { get; set; }

        // Closed doors block walking; open doors don't
        public override bool IsWalkable => IsOpen;

        // How the door looks
        public override string Glyph => IsOpen ? "/" : "+";

        public DoorObject(Point pos, DoorState state, bool initiallyOpen = false) : base(pos, "Door")
        {
            Pos = pos;
            Name = "Door";
            State = state;
        }

        public override void Interact(PlayerCharacter player, DungeonMap map,
                                      List<NonPlayerCharacter> npcs,
                                      Dictionary<Point, List<Item>> itemGrid)
        {
            if (IsLocked)
            {
                if (player.Keys > 0)
                {
                    player.Keys--;
                    State = DoorState.Closed;
                }
                return;
            }

            State = (State == DoorState.Open) ? DoorState.Closed : DoorState.Open;
        }


        // Doors are not inventory items, so ignore pickup/drop/equip
        protected override void OnPickedUp(PlayerCharacter player) { }
        public override void OnDropped(PlayerCharacter player) { }
        public override bool CanEquip => false;
        public override void OnEquip(PlayerCharacter player) { }
        public override void OnUnequip(PlayerCharacter player) { }
    }
}
