using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RoguelikeMonoGame
{
    public static class TileAnimationLibrary
    {
        // AnimationId -> AnimatedTile
        public static readonly Dictionary<byte, AnimatedTile> AnimatedTiles = new();

        public static void Load(ContentManager content)
        {
            // Example: animated water
            var tex = content.Load<Texture2D>("Tiles/Grassland/grassland_tileset");
            int tileSize = 32;

            // Suppose water frames are in row 2, columns 0..3:
            var frames = new Rectangle[4];
            for (int i = 0; i < 4; i++)
                frames[i] = new Rectangle(i * tileSize, 2 * tileSize, tileSize, tileSize);

            AnimatedTiles[1] = new AnimatedTile(tex, frames, 0.15f);
        }
    }
}
