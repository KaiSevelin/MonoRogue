using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    /// <summary>
    /// Generic outdoor region generator.
    /// 
    /// Right now it:
    /// - Optionally generates a "city" ground level with houses (using CityAlgorithm + HouseInteriorAlgorithm),
    ///   like your old GrasslandRegionGenerator.
    /// - Adds multi-floor house interiors as separate Levels.
    /// - Optionally creates a number of simple “dungeon portal” Levels and connects them.
    ///
    /// VegetationDensity / SeaCoverage are exposed through the config so you can later use them
    /// when placing tiles, props or special terrain.
    /// </summary>
    public sealed class OutdoorGenerator : IRegionGenerator
    {
        private readonly OutdoorConfig _cfg;

        public OutdoorGenerator(OutdoorConfig cfg)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        }

        public void GenerateRegion(Region region, Random rng)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            // Initialize region-level walkability; we’ll overwrite it with city’s Walkable at the end.
            region.Walkable = new bool[_cfg.Width, _cfg.Height];

            // --- 1) If no houses at all, make a simple "open field" outdoor Level and bail out ---
            if (!_cfg.HasHouses)
            {
                var lvl = new Level
                {
                    Id = "outdoor",
                    Walkable = new bool[_cfg.Width, _cfg.Height],
                    PlayerStart = new Point(_cfg.Width / 2, _cfg.Height / 2)
                };

                // For now: everything walkable. You can later carve lakes, trees, hills, etc.,
                // based on _cfg.VegetationDensity and _cfg.SeaCoverage.
                for (int y = 0; y < _cfg.Height; y++)
                    for (int x = 0; x < _cfg.Width; x++)
                        lvl.Walkable[x, y] = true;

                region.Levels.Add(lvl);
                region.Walkable = lvl.Walkable;
                return;
            }

            // --- 2) Generate city ground level (old GrasslandRegionGenerator logic) ---
            var cityCfg = new CityConfig();
            var cityGrid = new bool[_cfg.Width, _cfg.Height];
            new CityAlgorithm().Generate(cityGrid, rng, cityCfg);
            var cityLevel = cityCfg.CityLevel;
            region.Levels.Add(cityLevel);

            // --- 3) For each building, generate floors and connect them with stairs ---
            foreach (var b in cityCfg.Buildings)
            {
                List<Point>? stairsFromBelow = null;
                Level? prevFloorLevel = null;

                for (int floor = 0; floor < b.Floors; floor++)
                {
                    var houseGrid = new bool[b.Footprint.Width, b.Footprint.Height];
                    var hCfg = new HouseConfig
                    {
                        Building = b,
                        FloorIndex = floor,
                        StairsFromBelow = stairsFromBelow
                    };

                    new HouseInteriorAlgorithm().Generate(houseGrid, rng, hCfg);
                    var lvl = hCfg.OutputLevel;
                    region.Levels.Add(lvl);

                    // connections between floors
                    if (prevFloorLevel != null && stairsFromBelow != null && stairsFromBelow.Count > 0)
                    {
                        var upPos = stairsFromBelow[0];
                        var downPos = hCfg.StairsToAbove.Count > 0 ? hCfg.StairsToAbove[0] : upPos;

                        var upConn = new LevelConnection
                        {
                            FromLevelId = prevFloorLevel.Id,
                            FromPos = upPos,
                            ToLevelId = lvl.Id,
                            ToPos = downPos,
                            Type = ConnectionType.StairsUp
                        };
                        var downConn = new LevelConnection
                        {
                            FromLevelId = lvl.Id,
                            FromPos = downPos,
                            ToLevelId = prevFloorLevel.Id,
                            ToPos = upPos,
                            Type = ConnectionType.StairsDown
                        };
                        prevFloorLevel.Connections.Add(upConn);
                        lvl.Connections.Add(downConn);
                    }

                    prevFloorLevel = lvl;
                    stairsFromBelow = hCfg.StairsToAbove;
                }

                // Connect city entrance → building ground floor
                if (b.Floors > 0)
                {
                    string groundId = $"{b.Id}-floor-0";

                    var cityConn = new LevelConnection
                    {
                        FromLevelId = cityLevel.Id,
                        FromPos = b.Entrance,
                        ToLevelId = groundId,
                        ToPos = new Point(1, 1), // spawn just inside the house
                        Type = ConnectionType.BuildingEntrance
                    };

                    var backConn = new LevelConnection
                    {
                        FromLevelId = groundId,
                        FromPos = new Point(1, 1),
                        ToLevelId = cityLevel.Id,
                        ToPos = b.Entrance,
                        Type = ConnectionType.BuildingEntrance
                    };

                    cityLevel.Connections.Add(cityConn);
                    var groundLevel = region.GetLevelById(groundId);
                    if (groundLevel != null)
                        groundLevel.Connections.Add(backConn);
                }
            }

            // --- 4) Create N dungeon portals from the city into simple "dungeon" levels ---
            for (int i = 0; i < _cfg.DungeonPortals; i++)
            {
                var grid = cityLevel.Walkable;
                int w = grid.GetLength(0);
                int h = grid.GetLength(1);

                // Find a random walkable tile in the city for the portal.
                Point portalPos;
                int guard = 0;
                do
                {
                    portalPos = new Point(rng.Next(1, w - 1), rng.Next(1, h - 1));
                    guard++;
                } while (!grid[portalPos.X, portalPos.Y] && guard < 200);

                // Very simple placeholder dungeon: fully walkable box.
                var dungeon = new Level
                {
                    Id = $"dungeon-{i}",
                    Walkable = new bool[w, h],
                    PlayerStart = new Point(w / 2, h / 2)
                };
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        dungeon.Walkable[x, y] = true;

                region.Levels.Add(dungeon);

                var toDungeon = new LevelConnection
                {
                    FromLevelId = cityLevel.Id,
                    FromPos = portalPos,
                    ToLevelId = dungeon.Id,
                    ToPos = dungeon.PlayerStart,
                    Type = ConnectionType.Portal
                };
                var back = new LevelConnection
                {
                    FromLevelId = dungeon.Id,
                    FromPos = dungeon.PlayerStart,
                    ToLevelId = cityLevel.Id,
                    ToPos = portalPos,
                    Type = ConnectionType.Portal
                };

                cityLevel.Connections.Add(toDungeon);
                dungeon.Connections.Add(back);
            }

            // For now, expose city walkability as the region-level mask.
            // You can later mix this with lakes / vegetation based on _cfg.SeaCoverage / VegetationDensity.
            region.Walkable = cityLevel.Walkable;
        }
    }
}
