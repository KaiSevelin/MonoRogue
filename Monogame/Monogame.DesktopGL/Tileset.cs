using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public sealed class Tileset
    {
        public Texture2D Texture { get; }
        public int TileSize { get; }

        // basic ground tiles (base variant = mask 0)
        public Dictionary<(GroundType, int), Rectangle> GroundRects { get; } = new();

        // walls/cliffs/fences autotiled by mask
        public Dictionary<(WallType, int), Rectangle> WallRects { get; } = new();

        // generic sprites (trees, props, etc. if you want them later)
        public Dictionary<string, Rectangle> DecoRects { get; } = new();

        public Tileset(Texture2D texture, int tileSize)
        {
            Texture = texture;
            TileSize = tileSize;
        }
    }

}
