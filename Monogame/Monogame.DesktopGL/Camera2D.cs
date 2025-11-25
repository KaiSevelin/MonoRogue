
using Microsoft.Xna.Framework;

namespace RoguelikeMonoGame
{
    public class Camera2D
    {
        public Matrix Transform { get; set; }

        public void Follow(Vector2 targetPosition, int screenWidth, int screenHeight, int levelWidthPixels, int levelHeightPixels)
        {
            float x = MathHelper.Clamp(targetPosition.X,
                screenWidth / 2,
                levelWidthPixels - screenWidth / 2);

            float y = MathHelper.Clamp(targetPosition.Y,
                screenHeight / 2,
                levelHeightPixels - screenHeight / 2);

            Transform = Matrix.CreateTranslation(
                -x + screenWidth / 2,
                -y + screenHeight / 2,
                0);
        }
    }

}
