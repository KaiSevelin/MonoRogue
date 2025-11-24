using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeMonoGame;
using System;
using System.Collections.Generic;

public sealed class Tileset
{
    public Texture2D Texture { get; }
    public int TileSize { get; }

    public Dictionary<(GroundType, int), Rectangle> GroundRects { get; } = new();
    public Dictionary<(WallType, int), Rectangle> WallRects { get; } = new();
    public Dictionary<string, Rectangle> DecoRects { get; } = new();

    // OPTIONAL convenience fields
    public Rectangle DoorClosedRect { get; set; }
    public Rectangle DoorOpenRect { get; set; }
    public Rectangle StairsUpRect { get; set; }
    public Rectangle StairsDownRect { get; set; }
    public Rectangle[] TreeRects { get; set; } = Array.Empty<Rectangle>();
    public Rectangle[] RoofRects { get; set; } = Array.Empty<Rectangle>();

    public Tileset(Texture2D texture, int tileSize)
    {
        Texture = texture;
        TileSize = tileSize;
    }
}
