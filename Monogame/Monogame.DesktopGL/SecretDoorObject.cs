using Microsoft.Xna.Framework;
using RogueTest;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public class SecretDoorObject : DoorObject, IHiddenRevealable
    {
        public bool Discovered { get;  set; } = false;

        public SecretDoorObject(Point p)
            : base(p, DoorState.Closed) { }

        // closed secret door is a wall until discovered + opened
        public override bool IsWalkable => Discovered && (State == DoorState.Open);


        // IHiddenRevealable
        public void Reveal(DungeonMap map, PlayerCharacter searcher)
        {
            if (Discovered) return;
            Discovered = true;

            // Carve the wall to be a proper door tile
            map.ForceCarveFloor(Pos);
        }

        public override void Interact(
            PlayerCharacter player,
            DungeonMap map,
            List<NonPlayerCharacter> npcs,
            Dictionary<Point, List<Item>> itemGrid)
        {
            if (!Discovered)
            {
                Reveal(map, player);   // first interaction reveals
                return;
            }

            // Once discovered, behave like a normal door (open/close/lock)
            base.Interact(player, map, npcs, itemGrid);
        }
    }
}
