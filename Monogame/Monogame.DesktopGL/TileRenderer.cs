using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueTest;

namespace RoguelikeMonoGame
{
    /// <summary>
    /// Centralised draw logic for the world.
    /// It does not own game state – it only knows how to render given data.
    /// </summary>
    public sealed class TileRenderer
    {
        private readonly SpriteBatch _sb;
        private readonly int _tileSize;
        private readonly int _originX;   // offset for world rendering (because of UI)
        private readonly int _originY;

        public TileRenderer(SpriteBatch sb, int tileSize, int originX, int originY = 0)
        {
            _sb = sb;
            _tileSize = tileSize;
            _originX = originX;
            _originY = originY;
        }

        // These two callbacks let you plug in your existing rect/text helpers
        public Action<Rectangle, Color>? FillRect;
        public Action<string, int, int, Color, int>? DrawText;

        public void DrawWorld(
            DungeonMap map,
            PlayerCharacter player,
            IReadOnlyList<NonPlayerCharacter> npcs,
            Dictionary<Point, List<Item>> itemsAt,
            float[,] light)
        {
            int w = map.Width;
            int h = map.Height;

            // ---- FLOOR / WALL ----
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool vis = player.Visible?[x, y] ?? false;
                    bool exp = player.Explored?[x, y] ?? false;

                    var r = TileRect(x, y);

                    if (!exp)
                    {
                        FillRect?.Invoke(r, Color.Black);
                        continue;
                    }

                    bool isFloor = map.IsFloor(new Point(x, y));

                    var baseCol = isFloor
                        ? new Color(24, 24, 24)
                        : new Color(52, 52, 52);

                    float L = light[x, y];
                    L = Math.Clamp(L, 0f, 1f);

                    float k = vis ? Math.Max(0.65f, L) : 0.35f;

                    var col = new Color(
                        (byte)Math.Clamp(baseCol.R * k, 0, 255),
                        (byte)Math.Clamp(baseCol.G * k, 0, 255),
                        (byte)Math.Clamp(baseCol.B * k, 0, 255));

                    // explored-but-not-currently-visible tint
                    if (!vis)
                        col = new Color((byte)(col.R * .85f), (byte)(col.G * .95f + 15), (byte)(col.B + 25));

                    FillRect?.Invoke(r, col);
                }
            }

            // ---- STAIRS ----
            if (player.Visible?[map.Stairs.X, map.Stairs.Y] == true)
            {
                var r = TileRect(map.Stairs.X, map.Stairs.Y);
                FillRect?.Invoke(r, new Color(15, 15, 25));
                DrawText?.Invoke(">", r.X + 6, r.Y + 2, Color.LightSteelBlue, 18);
            }

            // ---- ITEMS / DOORS ----
            foreach (var kv in itemsAt)
            {
                var p = kv.Key;
                var list = kv.Value;

                if (!map.InBounds(p)) continue;
                if (!(player.Visible?[p.X, p.Y] ?? false)) continue;

                // Filter out hidden items
                var visibleItems = list.Where(it =>
                    it is not IHiddenRevealable h || h.Discovered).ToList();

                if (visibleItems.Count == 0)
                    continue;

                var r = TileRect(p.X, p.Y);

                // Prefer doors visually if present
                var door = visibleItems.OfType<DoorObject>().FirstOrDefault();
                if (door != null)
                {
                    Color color;
                    if (door is SecretDoorObject s && !s.Discovered)
                        color = new Color(70, 70, 70);   // disguised as wall
                    else
                        color = new Color(150, 110, 60); // visible door

                    DrawText?.Invoke(door.Glyph, r.X + 6, r.Y + 2, color, 18);
                    continue;
                }

                // No door: either single item or pile
                if (visibleItems.Count == 1)
                {
                    var it = visibleItems[0];
                    DrawText?.Invoke(it.Glyph, r.X + 6, r.Y + 2, Color.Gold, 18);
                }
                else
                {
                    // simple pile representation
                    FillRect?.Invoke(
                        new Rectangle(r.X + 6, r.Y + 10, 12, 6),
                        new Color(120, 90, 40));

                    DrawText?.Invoke(visibleItems.Count.ToString(),
                                     r.X + 4, r.Y + 1, Color.Wheat, 14);
                }
            }



            // ---- NPCs ----
            foreach (var e in npcs)
            {
                if (!map.InBounds(e.Pos)) continue;
                if (!(player.Visible?[e.Pos.X, e.Pos.Y] ?? false)) continue;

                var r = TileRect(e.Pos.X, e.Pos.Y);

                // If you have AnimatedSprites on NPCs, call them here instead of rectangles
                // For now, keep your color blocks:
                var bodyColor = e.Kind == NpcKind.SkeletonArcher
                    ? new Color(80, 130, 60)
                    : new Color(120, 40, 40);

                FillRect?.Invoke(r, bodyColor);
                DrawText?.Invoke(
                    e.Kind == NpcKind.SkeletonArcher ? "a" : "o",
                    r.X + 6, r.Y + 2, Color.Wheat, 18);
            }

            // ---- PLAYER ----
            {
                var r = TileRect(player.Pos.X, player.Pos.Y);
                FillRect?.Invoke(r, new Color(50, 100, 160));
                DrawText?.Invoke("@", r.X + 5, r.Y + 2, Color.White, 18);
            }
        }

        Rectangle TileRect(int x, int y)
        {
            return new Rectangle(
                _originX + x * _tileSize,
                _originY + y * _tileSize,
                _tileSize,
                _tileSize);
        }
    }
}
