using Microsoft.Xna.Framework;
using RogueTest;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class NonPlayerCharacter : Character
    {
        public readonly Guid Id = Guid.NewGuid();
        public NpcKind Kind;
        public WeaponItem? EquippedWeapon;
        public NpcAI AI;

        public NonPlayerCharacter(Point pos, NpcKind kind = NpcKind.Orc, NpcAI? ai = null) : base(pos, 30)
        {
            Kind = kind;

            AI = ai ?? new NpcAI(NpcBehavior.Hostile);
        }

        public override void Interact(PlayerCharacter player, DungeonMap map, List<NonPlayerCharacter> npcs, Dictionary<Point, List<Item>> itemsAt)
        {
            //Talk to npc TODO
        }

        public void TakeTurn(World world, DungeonMap map, PlayerCharacter player, List<NonPlayerCharacter> all, Random rng)
        {
            AI.TakeTurn(world, this, map, player, all, rng);
        }
    }
}
