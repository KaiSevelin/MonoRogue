using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class ArrowTrap : Trap
    {
        public Point Direction { get; }

        public int Damage { get; }

        public ArrowTrap(Point pos, Point dir, int damage = 8)
            : base(pos, "Arrow Trap")
        {
            Direction = dir;
            Damage = damage;
        }

        public override string Glyph => "t";

        public override void Trigger(Character target, DungeonMap map, List<NonPlayerCharacter> npcs)
        {
            if (!IsArmed) return;

            // Fire a projectile along Direction
            // You already have Projectile + world.AdvanceTurn – reuse that
            IsArmed = false;
            Discovered = true;
        }
    }
}

