using System.Collections.Generic;
using System.Linq;

namespace RoguelikeMonoGame
{
    public sealed class Region
    {
        public string Id { get; }
        public TerrainType Terrain { get; }
        public string CountryId { get; }

        // 2D map for this region (overland or dungeon)
        public bool[,] Walkable;              // for IDungeonAlgorithm.Generate
        public List<Level> Levels = new();    // house floors, towers, dungeons, etc.

        public Region(string id, TerrainType terrain, string countryId)
        {
            Id = id;
            Terrain = terrain;
            CountryId = countryId;
        }
        public Level? GetLevelById(string id)
        {
            return Levels.FirstOrDefault(l => l.Id == id);
        }
    }

}
