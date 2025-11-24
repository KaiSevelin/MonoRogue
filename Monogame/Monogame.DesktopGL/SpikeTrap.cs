using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class SpikeTrap : Trap
    {
        public int Damage { get; }

        public SpikeTrap(Point pos, int damage = 10)
            : base(pos, "Spike Trap")
        {
            Damage = damage;
        }

        public override string Glyph => "^";

        public override void Trigger(Character target, DungeonMap map, List<NonPlayerCharacter> npcs)
        {
            if (!IsArmed) return;

            target.TakeDamage(Damage);
            IsArmed = false;        // one-use, or keep true for repeating trap
            Discovered = true;       // now obvious
        }
    }
}

