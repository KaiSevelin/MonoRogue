using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoguelikeMonoGame
{
    public sealed class AnimatedSprite
    {
        public Texture2D Texture { get; }
        public Rectangle[] Frames { get; }
        public float FrameDuration;

        int _current;
        float _time;

        public AnimatedSprite(Texture2D texture, Rectangle[] frames, float frameDuration)
        {
            Texture = texture;
            Frames = frames;
            FrameDuration = frameDuration;
        }

        public void Update(float dt)
        {
            _time += dt;
            while (_time >= FrameDuration)
            {
                _time -= FrameDuration;
                _current = (_current + 1) % Frames.Length;
            }
        }

        public void Reset()
        {
            _current = 0;
            _time = 0;
        }

        public void Draw(SpriteBatch sb, Rectangle dest, Color color)
        {
            sb.Draw(Texture, dest, Frames[_current], color);
        }
    }

}
