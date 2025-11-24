using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RoguelikeMonoGame
{
    /// <summary>
    /// High-level operations for doors:
    /// - Interact (open/close/lock/reveal)
    /// - Search for secret doors around the player
    /// 
    /// Low-level passability/opacity is still handled by DungeonMap.
    /// </summary>
    public sealed class DoorSystem
    {
        /// <summary>
        /// Try to interact with a door at the tile in front of the player.
        /// Returns true if a door was found and interacted with.
        /// </summary>
        public bool TryInteractAhead(
            PlayerCharacter player,
            DungeonMap map,
            List<NonPlayerCharacter> npcs)
        {
            var ahead = new Point(player.Pos.X + player.Facing.X,
                                  player.Pos.Y + player.Facing.Y);

            if (!map.InBounds(ahead)) return false;

            // First: secret-door reveal on the wall position

            // Normal door
            if (map.Doors.TryGetValue(ahead, out var door))
            {
                door.Interact(player, map, npcs, map.ItemsAt);
                return true;
            }

            // Doors stored as items (for secret doors placed in ItemsAt)
            if (map.ItemsAt.TryGetValue(ahead, out var list))
            {
                foreach (var it in list)
                {
                    if (it is DoorObject d)
                    {
                        d.Interact(player, map, npcs, map.ItemsAt);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Search around the player (within radius) for secret doors.
        /// Each found secret door is revealed.
        /// Returns the number of doors revealed.
        /// </summary>
        public int SearchForSecretDoors(PlayerCharacter player, DungeonMap map, int radius = 2)
        {
            int found = 0;
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    var p = new Point(player.Pos.X + dx, player.Pos.Y + dy);
                    if (!map.InBounds(p)) continue;

                    if (map.Doors.TryGetValue(p, out var d) && d is SecretDoorObject s && !s.Discovered)
                    {
                        s.Reveal(map, player);
                        found++;
                    }

                    if (map.ItemsAt.TryGetValue(p, out var list))
                    {
                        foreach (var it in list)
                        {
                            if (it is SecretDoorObject s2 && !s2.Discovered)
                            {
                                s2.Reveal(map, player);
                                found++;
                            }
                        }
                    }
                }
            }
            return found;
        }
    }
}

