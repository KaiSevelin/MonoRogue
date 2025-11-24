using Microsoft.Xna.Framework;
using RogueTest;
using System;

namespace RoguelikeMonoGame
{
    public sealed class KeyItem : Item
    {
        // Spawn a key on the map
        public KeyItem(Point p) : base(p, "Key") { }

        // Or create one directly in inventory (-1,-1)
        public KeyItem() : base("Key") { }

        public override string Glyph => "k";

        protected override void OnPickedUp(PlayerCharacter player) => player.Keys++;

        public override void OnDropped(PlayerCharacter player)
            => player.Keys = Math.Max(0, player.Keys - 1);
    }



}
