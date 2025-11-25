
using Microsoft.Xna.Framework;

namespace RoguelikeMonoGame
{
    public class Camera2D
    {
        public Matrix Transform { get; set; }

        public void Follow(Vector2 target, int screenWidth, int levelWidthPixels)
        {
            float cameraX = target.X - screenWidth / 2f;  // center player

            cameraX = MathHelper.Clamp(cameraX, 0, levelWidthPixels - screenWidth);

            Transform = Matrix.CreateTranslation(-cameraX, 0, 0);
        }
    }

}
