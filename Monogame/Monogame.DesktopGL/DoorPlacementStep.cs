// RogueTest/DoorPlacementStep.cs
using Microsoft.Xna.Framework;
using RogueTest;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    // Door placement that can also place secret doors.
    public sealed class DoorPlacementStep : IGenStep
    {
        public string Name => "Door Placement";

        private readonly DungeonMap _map;      // record doors into live map
        private readonly int _maxDoors;
        private readonly int _secretPct;       // % chance that a door is secret

        public DoorPlacementStep(DungeonMap map, int maxDoors, int secretPct = 10)
        {
            _map = map;
            _maxDoors = Math.Max(0, maxDoors);
            _secretPct = Math.Clamp(secretPct, 0, 100);
        }

        public void Run(bool[,] grid, Random rng)
        {
            int W = grid.GetLength(0), H = grid.GetLength(1);

            // 1) Find all potential door locations: wall tile with floor on opposite sides
            var candidates = new List<Point>();
            for (int y = 1; y < H - 1; y++)
                for (int x = 1; x < W - 1; x++)
                {
                    if (grid[x, y]) continue; // must currently be wall

                    bool horiz = grid[x - 1, y] && grid[x + 1, y] && !grid[x, y - 1] && !grid[x, y + 1];
                    bool vert = grid[x, y - 1] && grid[x, y + 1] && !grid[x - 1, y] && !grid[x + 1, y];

                    if (horiz || vert)
                        candidates.Add(new Point(x, y));
                }

            Shuffler.Shuffle(candidates.ToArray(), rng);

            // 3) Place doors, but never closer than 1 tile from each other
            var blocked = new bool[W, H]; // marks a 3×3 area around each placed door

            int placed = 0;
            foreach (var c in candidates)
            {
                if (placed >= _maxDoors)
                    break;

                if (blocked[c.X, c.Y])
                    continue; // too close to an existing door

                bool placeSecret = rng.Next(100) < _secretPct;

                if (placeSecret)
                {
                    // Secret door: keep grid[x,y] = false (wall) and register a SecretDoorObject.
                    if (!_map.Doors.ContainsKey(c))
                    {
                        _map.Doors[c] = new SecretDoorObject(c);
                        placed++;
                    }
                }
                else
                {
                    // Normal door: carve to floor and place closed door
                    grid[c.X, c.Y] = true; // now floor
                    if (!_map.Doors.ContainsKey(c))
                    {
                        _map.Doors[c] = new DoorObject(c, DoorState.Closed);
                        placed++;
                    }
                }

                // If we actually placed a door, mark the surrounding 3×3 area as blocked
                if (_map.Doors.ContainsKey(c))
                {
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = c.X + dx;
                            int ny = c.Y + dy;
                            if (nx >= 0 && ny >= 0 && nx < W && ny < H)
                                blocked[nx, ny] = true;
                        }
                }
            }
        }
    }
}
