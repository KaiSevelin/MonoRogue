using System;
using System.Linq.Expressions;

namespace RoguelikeMonoGame
{
    public static class DungeonMapFactory
    {
        public static DungeonMap CreateRandomDungeon(
            int width,
            int height,
            Random rng)
        {
            // 1) use your map algorithms to fill a bool[,] grid
            var grid = new bool[width, height];

            IDungeonAlgorithm[] algos =
            {
            new BSPRoomsAlgorithm(),
            new RoomsAndPillarsAlgorithm(),
            new CellularCavesAlgorithm(),
            new MazeDFSAlgorithm(),
            new TowerAlgorithm()
        };

            var algo = algos[rng.Next(algos.Length)];
            IDungeonConfig cfg = algo is BSPRoomsAlgorithm ? new BspRoomsConfig()
                             : algo is RoomsAndPillarsAlgorithm ? new RoomsPillarsConfig()
                             : algo is MazeDFSAlgorithm ? new MazeConfig()
                             : algo is TowerAlgorithm ? new TowerConfig()
                             : new CellularConfig();

            algo.Generate(grid, rng, cfg);  // fills grid[x,y] = floor?   

            // 2) build DungeonMap from that
            var map = new DungeonMap(width, height);
            map.LoadFromWalkable(grid);

            // 3) run shared finishing steps (doors, connectivity, loops, etc.)
            new EnsureConnectivityStep().Run(grid, rng);
            new AddLoopsStep(20).Run(grid, rng);
            map.PlaceDoors(rng, maxDoors: 20, secretChancePercent: 15);

            // 4) choose start/stairs
            map.PlayerStart = map.RandomFloor(rng);
            map.Stairs = map.RandomFloor(rng, minDistance: width / 3);

            //5 Place traps
            map.PlaceRandomTraps(rng,  (width * height) / 100);
            return map;
        }
    }

}
