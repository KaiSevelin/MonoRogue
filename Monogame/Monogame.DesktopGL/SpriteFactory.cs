using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RoguelikeMonoGame;

public abstract partial class Character
{
    public static class SpriteFactory
    {
        public static Texture2D OrcTexture;
        public static Texture2D SkeletonTexture;

        public static void Load(ContentManager content)
        {
            OrcTexture = content.Load<Texture2D>("Sprites/Orc/orc-sheet");
            SkeletonTexture = content.Load<Texture2D>("Sprites/Skeleton/skeleton-sheet");
        }

        public static void SetupOrcAnimations(NonPlayerCharacter npc)
        {
            const int frameSize = 32;
            const float frameDuration = 0.12f;

            // Example: assume orc sheet rows:
            // Row 0: idle (4 frames, facing down)
            // Row 1: walk (6 frames, facing down)
            // Row 2: walk (6 frames, facing left)
            // Row 3: walk (6 frames, facing right)
            // Row 4: walk (6 frames, facing up)
            // (You’ll need to adapt to the actual layout.)

            AnimatedSprite MakeRow(int row, int frames)
            {
                var rects = new Rectangle[frames];
                for (int i = 0; i < frames; i++)
                    rects[i] = new Rectangle(i * frameSize, row * frameSize, frameSize, frameSize);
                return new AnimatedSprite(OrcTexture, rects, frameDuration);
            }

            // idle – just one row/down (you could duplicate for directions)
            npc.Animations[(AnimState.Idle, FacingDir.Down)] = MakeRow(0, 4);

            // walk down/left/right/up
            npc.Animations[(AnimState.Walk, FacingDir.Down)] = MakeRow(1, 6);
            npc.Animations[(AnimState.Walk, FacingDir.Left)] = MakeRow(2, 6);
            npc.Animations[(AnimState.Walk, FacingDir.Right)] = MakeRow(3, 6);
            npc.Animations[(AnimState.Walk, FacingDir.Up)] = MakeRow(4, 6);

            // For attack, pick rows with attack animations if you like
        }

        public static void SetupSkeletonArcherAnimations(NonPlayerCharacter npc)
        {
            const int frameSize = 32;
            const float frameDuration = 0.12f;

            AnimatedSprite MakeRow(Texture2D tex, int row, int frames)
            {
                var rects = new Rectangle[frames];
                for (int i = 0; i < frames; i++)
                    rects[i] = new Rectangle(i * frameSize, row * frameSize, frameSize, frameSize);
                return new AnimatedSprite(tex, rects, frameDuration);
            }

            // Example: use SkeletonTexture with similar layout
            npc.Animations[(AnimState.Idle, FacingDir.Down)] =
                MakeRow(SkeletonTexture, 0, 4);
            npc.Animations[(AnimState.Walk, FacingDir.Down)] =
                MakeRow(SkeletonTexture, 1, 6);
            // Add other directions as needed...
        }
    }

}
