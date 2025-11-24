using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class StabUse : IUse
    {
        public int Damage => _damage;

        public string Id => "Stab";
        public string Label => "Stab";
        readonly int _damage;

        public StabUse(int damage) { _damage = damage; }

        public bool Perform(Character user, DungeonMap map, List<NonPlayerCharacter> npcs,
                            Point dir, Random rng)
        {
            if (dir == Point.Zero) dir = user.Facing;
            var tgt = new Point(user.Pos.X + dir.X, user.Pos.Y + dir.Y);
            var enemy = npcs.Find(n => n.Pos == tgt);
            if (enemy == null) return false;
            enemy.TakeDamage(_damage);
            return true;
        }
    }
}
