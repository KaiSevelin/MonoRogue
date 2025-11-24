using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace RoguelikeMonoGame
{
    /// <summary>
    /// Contains core world/simulation logic: advancing turns and pathfinding.
    /// Keeps Game1 focused on input + rendering.
    /// </summary>
    public sealed class World
    {
        public ContinentState State { get; }

        public World(int worldW, int worldH, int regionW, int regionH, Random rng)
        {
            var continent = new Continent(worldW, worldH);
            continent.InitializeRegions(regionW, regionH, rng);

            // Start in (0,0) region, first level
            var region = continent.Regions[0, 0];
            var level = region.Levels[0];

            State = new ContinentState(continent, region, level);
        }
        /// <summary>
        /// Advance one full game turn:
        /// - step all projectiles
        /// - let all NPCs act (AI + movement + attacks)
        /// Does NOT touch FOV/UI or game-over state; Game1 remains responsible for that.
        /// </summary>
        public void AdvanceTurn(
            DungeonMap map,
            PlayerCharacter player,
            List<NonPlayerCharacter> enemies,
            Random rng)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (enemies == null) throw new ArgumentNullException(nameof(enemies));
            if (rng == null) throw new ArgumentNullException(nameof(rng));


            // 2) Enemies take their turns (AI, movement, attacks)
            foreach (var e in enemies.ToArray()) // snapshot: AI may remove enemies
            {
                e.TakeTurn(this, map, player, enemies, rng);
                HandleStepOnTraps(e, map, enemies);
                if (e.IsDead)
                    enemies.Remove(e);
            }
            // Note: do NOT set player dead / game-over here.
            // Game1 should check player.IsDead and handle UI or restart.
            // 3) Handle player stepping on traps
        }
        public void HandleStepOnTraps(Character ch, DungeonMap map, List<NonPlayerCharacter> npcs)
        {
            var trap = map.GetTrapAt(ch.Pos);
            if (trap == null) return;

            if (!trap.IsArmed) return;

            trap.Trigger(ch, map, npcs);

            // If trap is one-shot and should be removed:
            if (!trap.IsArmed)
                map.Traps.Remove(ch.Pos);
        }
        /// <summary>
        /// Simple BFS pathfinding from start to goal:
        /// - respects map passability
        /// - avoids enemy-occupied tiles, except it allows stepping onto the goal
        /// Returns a list of points from start to goal, or null if unreachable.
        /// </summary>
        public List<Point>? FindPath(
            DungeonMap map,
            Point start,
            Point goal,
            List<NonPlayerCharacter> enemies,
            int sizeW = 1,
            int sizeH = 1,
            NonPlayerCharacter? self = null)
        {
            // Local helper: can our footprint stand with top-left at p?
            bool CanStandAt(Point p)
            {
                var rect = new Rectangle(p.X, p.Y, sizeW, sizeH);
                return map.IsAreaPassable(rect, enemies, self);
            }

            if (!map.InBounds(start) || !map.InBounds(goal))
                return null;

            if (!CanStandAt(start) || !CanStandAt(goal))
                return null;

            var dirs = new[]
            {
        new Point(1, 0),
        new Point(-1, 0),
        new Point(0, 1),
        new Point(0, -1)
    };

            var cameFrom = new Dictionary<Point, Point>();
            var q = new Queue<Point>();
            var visited = new HashSet<Point>();

            q.Enqueue(start);
            visited.Add(start);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (cur == goal)
                    break;

                foreach (var d in dirs)
                {
                    var np = new Point(cur.X + d.X, cur.Y + d.Y);
                    if (!map.InBounds(np)) continue;
                    if (visited.Contains(np)) continue;
                    if (!CanStandAt(np)) continue;

                    visited.Add(np);
                    cameFrom[np] = cur;
                    q.Enqueue(np);
                }
            }

            if (!cameFrom.ContainsKey(goal))
                return null;

            var path = new List<Point> { goal };
            var curPos = goal;
            while (curPos != start)
            {
                curPos = cameFrom[curPos];
                path.Add(curPos);
            }
            path.Reverse();
            return path;
        }
    }
}
