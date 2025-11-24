using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class SlashUse : IUse
    {
        public int Damage => _damage;

        public string Id => "Slash";
        public string Label => "Slash";
        readonly int _damage;

        public SlashUse(int damage) { _damage = damage; }

        public bool Perform(Character user, DungeonMap map, List<NonPlayerCharacter> npcs,
                            Point dir, Random rng)
        {
            if (dir == Point.Zero) dir = user.Facing;
            // Simple arc: target front tile and the two side-adjacent tiles
            var targets = new List<Point>
        {
            new Point(user.Pos.X + dir.X, user.Pos.Y + dir.Y),
            new Point(user.Pos.X + dir.Y, user.Pos.Y - dir.X),
            new Point(user.Pos.X - dir.Y, user.Pos.Y + dir.X)
        };
            bool hit = false;
            foreach (var p in targets)
            {
                var e = npcs.Find(n => n.Pos == p);
                if (e != null) { e.TakeDamage(_damage); hit = true; }
            }
            return hit;
        }
    }
}
