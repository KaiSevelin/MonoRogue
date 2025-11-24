using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeMonoGame
{
    public sealed class AnimatedTile
    {
        public Texture2D Texture { get; }
        public Rectangle[] Frames { get; }
        public float FrameDuration { get; }   // seconds per frame

        private float _time;
        private int _index;

        public AnimatedTile(Texture2D texture, Rectangle[] frames, float frameDuration)
        {
            Texture = texture;
            Frames = frames;
            FrameDuration = frameDuration;
            _time = 0f;
            _index = 0;
        }

        public void Update(float dt)
        {
            _time += dt;
            while (_time >= FrameDuration)
            {
                _time -= FrameDuration;
                _index = (_index + 1) % Frames.Length;
            }
        }

        public void Draw(SpriteBatch sb, Rectangle dest, Color color)
        {
            sb.Draw(Texture, dest, Frames[_index], color);
        }
    }
}
