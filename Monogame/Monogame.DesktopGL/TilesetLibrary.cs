using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public static class TilesetLibrary
    {
        public static readonly Dictionary<ThemeId, Tileset> Tilesets = new();

        public static void Load(ContentManager content)
        {
            LoadGrassland(content);
            // Load other themes later
        }

        static void LoadGrassland(ContentManager content)
        {
            var tex = content.Load<Texture2D>("Tiles/Grassland/rogue_tileset_16x16");
            const int tileSize = 16;

            var ts = new Tileset(tex, tileSize);

            // === GroundType mappings ===
            // Grass variants
            ts.GroundRects[(GroundType.Grass, 0)] = new Rectangle(0 * tileSize, 0 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Grass, 1)] = new Rectangle(1 * tileSize, 0 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Grass, 2)] = new Rectangle(2 * tileSize, 0 * tileSize, tileSize, tileSize);

            // Dirt variants
            ts.GroundRects[(GroundType.Dirt, 0)] = new Rectangle(3 * tileSize, 0 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Dirt, 1)] = new Rectangle(4 * tileSize, 0 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Dirt, 2)] = new Rectangle(5 * tileSize, 0 * tileSize, tileSize, tileSize);

            // Stone floor variants
            ts.GroundRects[(GroundType.Stone, 0)] = new Rectangle(6 * tileSize, 0 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Stone, 1)] = new Rectangle(7 * tileSize, 0 * tileSize, tileSize, tileSize);

            // Water variants
            ts.GroundRects[(GroundType.Water, 0)] = new Rectangle(0 * tileSize, 1 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Water, 1)] = new Rectangle(1 * tileSize, 1 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Water, 2)] = new Rectangle(2 * tileSize, 1 * tileSize, tileSize, tileSize);

            // Sand variants
            ts.GroundRects[(GroundType.Sand, 0)] = new Rectangle(3 * tileSize, 1 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Sand, 1)] = new Rectangle(4 * tileSize, 1 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Sand, 2)] = new Rectangle(5 * tileSize, 1 * tileSize, tileSize, tileSize);

            // Lava variants
            ts.GroundRects[(GroundType.Lava, 0)] = new Rectangle(6 * tileSize, 1 * tileSize, tileSize, tileSize);
            ts.GroundRects[(GroundType.Lava, 1)] = new Rectangle(7 * tileSize, 1 * tileSize, tileSize, tileSize);

            // === WallType mappings ===
            ts.WallRects[(WallType.Brick, 0)] = new Rectangle(0 * tileSize, 2 * tileSize, tileSize, tileSize);
            ts.WallRects[(WallType.Brick, 1)] = new Rectangle(1 * tileSize, 2 * tileSize, tileSize, tileSize);

            ts.WallRects[(WallType.Rock, 0)] = new Rectangle(2 * tileSize, 2 * tileSize, tileSize, tileSize);
            ts.WallRects[(WallType.Rock, 1)] = new Rectangle(3 * tileSize, 2 * tileSize, tileSize, tileSize);

            ts.WallRects[(WallType.Wood, 0)] = new Rectangle(4 * tileSize, 2 * tileSize, tileSize, tileSize);
            ts.WallRects[(WallType.Wood, 1)] = new Rectangle(5 * tileSize, 2 * tileSize, tileSize, tileSize);

            // Doors
            ts.DoorClosedRect = new Rectangle(6 * tileSize, 2 * tileSize, tileSize, tileSize);
            ts.DoorOpenRect = new Rectangle(7 * tileSize, 2 * tileSize, tileSize, tileSize);

            // Stairs
            ts.StairsUpRect = new Rectangle(0 * tileSize, 3 * tileSize, tileSize, tileSize);
            ts.StairsDownRect = new Rectangle(1 * tileSize, 3 * tileSize, tileSize, tileSize);

            // Trees
            ts.TreeRects = new[]
            {
        new Rectangle(2 * tileSize, 3 * tileSize, tileSize, tileSize),
        new Rectangle(3 * tileSize, 3 * tileSize, tileSize, tileSize)
    };

            // Roofs
            ts.RoofRects = new[]
            {
        new Rectangle(4 * tileSize, 3 * tileSize, tileSize, tileSize),
        new Rectangle(5 * tileSize, 3 * tileSize, tileSize, tileSize)
    };

            Tilesets[ThemeId.Grassland] = ts;
        }

    }

}
