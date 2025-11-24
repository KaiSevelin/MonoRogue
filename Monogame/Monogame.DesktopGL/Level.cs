using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeMonoGame
{
    // One logical map/floor
    public sealed class Level
    {
        public string Id;          // e.g. "city", "house-3-floor-1"
        public bool[,] Walkable;   // used by your DungeonMap
        public List<LevelConnection> Connections = new();
        public List<RoomInfo> Rooms = new();
        public List<CorridorInfo> Corridors = new();
        public List<DoorPlacement> Doors = new();
        public List<WallArea> WallAreas = new();
        public TileCell[,] Tiles;
        public Point? StairsDown;
        public Point PlayerStart { get; internal set; }
        public ThemeId Theme;


        public LevelConnection? FindConnectionAt(Point pos)
        {
            return Connections.FirstOrDefault(c => c.FromPos == pos);
        }
    }

}
