using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace RoguelikeMonoGame
{
    public sealed class NpcAI
    {
        public NpcBehavior Behavior;
        public Guid? LeaderId;

        public NpcAI(NpcBehavior b, Guid? leader = null)
        {
            Behavior = b; LeaderId = leader;
        }

        public void TakeTurn(World world, NonPlayerCharacter self, DungeonMap map, PlayerCharacter player,
                             List<NonPlayerCharacter> all, Random rng)
        {
            switch (Behavior)
            {
                case NpcBehavior.Immobilized:
                    return;

                case NpcBehavior.Confused:
                    TryRandomStep(self, map, player, all, rng);
                    return;

                case NpcBehavior.Neutral:
                    if (rng.NextDouble() < 0.5) TryRandomStep(self, map, player, all, rng);
                    return;

                case NpcBehavior.Scared:
                    StepAwayFrom(self, player.Pos, map, all);
                    return;

                case NpcBehavior.Follow:
                    {
                        var leader = FindLeader(all);
                        var target = leader?.Pos ?? player.Pos;
                        StepTowards(world, self, target, map, player, all);
                        return;
                    }

                case NpcBehavior.Pack:
                    {
                        var leader = FindLeader(all);
                        if (leader == null) { TryRandomStep(self, map, player, all, rng); return; }

                        if (leader == self)
                        {
                            // Leader behaves Hostile
                            HostileAct(world, self, map, player, all,  rng);
                        }
                        else
                        {
                            StepTowards(world, self, leader.Pos, map, player, all);
                        }
                        return;
                    }

                case NpcBehavior.Hostile:
                default:
                    HostileAct(world, self, map, player, all,  rng);
                    return;
            }
        }

        NonPlayerCharacter FindLeader(List<NonPlayerCharacter> all)
        {
            if (LeaderId == null || LeaderId == Guid.Empty) return null;
            foreach (var n in all) if (n.Id == LeaderId.Value) return n;
            return null;
        }

        void HostileAct(World world,NonPlayerCharacter self, DungeonMap map, PlayerCharacter player,
                        List<NonPlayerCharacter> all, Random rng)
        {
            int manhattan = Math.Abs(self.Pos.X - player.Pos.X) + Math.Abs(self.Pos.Y - player.Pos.Y);

            // Adjacent melee
            if (manhattan == 1)
            {
                if (self.EquippedWeapon != null)
                {
                    var dir = new Point(Math.Sign(player.Pos.X - self.Pos.X),
                                        Math.Sign(player.Pos.Y - self.Pos.Y));

                    if (self.EquippedWeapon.PerformUse("Slash", self, map, all,  dir, rng)) return;
                    if (self.EquippedWeapon.PerformUse("Stab", self, map, all,  dir, rng)) return;
                }
                player.TakeDamage(1);
                return;
            }

            // Ranged “Shoot”
            if (self.EquippedWeapon != null)
            {
                bool sameRowCol = (self.Pos.X == player.Pos.X || self.Pos.Y == player.Pos.Y);
                if (sameRowCol)
                {
                    int dist = manhattan;
                    if (dist <= 10 && map.HasLineOfSight(self.Pos, player.Pos))
                    {
                        var dir = new Point(Math.Sign(player.Pos.X - self.Pos.X),
                                            Math.Sign(player.Pos.Y - self.Pos.Y));

                        if (self.EquippedWeapon.PerformUse("Shoot", self, map, all, dir, rng)) return;
                    }
                }
            }

            // Move towards player
            StepTowards(world, self, player.Pos, map, player, all);
        }

        void TryRandomStep(NonPlayerCharacter self, DungeonMap map, PlayerCharacter player,
                           List<NonPlayerCharacter> all, Random rng)
        {
            var dirs = new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };
            for (int attempt = 0; attempt < 4; attempt++)
            {
                var d = dirs[rng.Next(dirs.Length)];
                var np = new Point(self.Pos.X + d.X, self.Pos.Y + d.Y);
                if (!map.IsPassable(np)) continue;
                if (np == player.Pos) continue; // DON'T step onto player in confused/follow/pack/etc
                if (all.Exists(e => e != self && e.Pos == np)) continue;
                self.Pos = np;
                return;
            }
            // no move if all blocked
        }

        void StepTowards(World world, NonPlayerCharacter self, Point target,
                         DungeonMap map, PlayerCharacter player,
                         List<NonPlayerCharacter> all)
        {
            var step = NextStep(self, world, self.Pos, target, map, all);
            if (!step.HasValue) return;

            var s = step.Value;
            if (s == player.Pos) return;                       // don't step onto player
            if (all.Exists(o => o != self && o.Pos == s)) return;
            self.Pos = s;
        }

        void StepAwayFrom(NonPlayerCharacter self, Point danger, DungeonMap map, List<NonPlayerCharacter> all)
        {
            var best = self.Pos; int bestDist = -1;
            var dirs = new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };
            foreach (var d in dirs)
            {
                var np = new Point(self.Pos.X + d.X, self.Pos.Y + d.Y);
                if (!map.IsPassable(np)) continue;
                if (all.Exists(e => e.Pos == np && e != self)) continue;
                int dist = Math.Abs(np.X - danger.X) + Math.Abs(np.Y - danger.Y);
                if (dist > bestDist) { bestDist = dist; best = np; }
            }
            if (best != self.Pos) self.Pos = best;
        }

        static Point? NextStep(NonPlayerCharacter subject,World world, Point from, Point to,
                               DungeonMap map, List<NonPlayerCharacter> all)
        {
            var path = world.FindPath(
                map,
                from,
                to,
                all,
                subject.SizeW,
                subject.SizeH,
                subject);
            if (path == null || path.Count < 2)
                return null;            // no path, or already at goal

            // path[0] is 'from', path[1] is the next tile to step into
            return path[1];
        }
    }
}
