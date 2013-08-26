using System.Collections.ObjectModel;
using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public class CannonRenderer : FlaiRenderer
    {
        private readonly ReadOnlyCollection<Cannon> _cannons;
        public CannonRenderer(ReadOnlyCollection<Cannon> cannons)
        {
            _cannons = cannons;
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            foreach (Cannon cannon in _cannons)
            {
                this.DrawCannon(graphicsContext, cannon);
            }

            foreach (Cannon cannon in _cannons)
            {
                this.DrawBullets(graphicsContext, cannon.Bullets);
            }
        }

        private void DrawCannon(GraphicsContext graphicsContext, Cannon cannon)
        {
            Texture2D cannonTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Cannon");
            graphicsContext.SpriteBatch.DrawCentered(cannonTexture, cannon.CenterPosition, Color.White, cannon.Rotation, 2);
        }

        private void DrawBullets(GraphicsContext graphicsContext, ReadOnlyCollection<Bullet> bullets)
        {
            foreach (Bullet bullet in bullets)
            {
                float scaleAlpha = FlaiMath.Max(0.25F, bullet.Alpha * bullet.Alpha);
                graphicsContext.SpriteBatch.DrawCentered(graphicsContext.BlankTexture, bullet.Position, new Color(228, 228, 288) * bullet.Alpha, bullet.Rotation, Bullet.Size * (bullet.IsAlive ? scaleAlpha : (2 * (1-scaleAlpha))));
                //graphicsContext.PrimitiveRenderer.DrawRectangleOutlines(graphicsContext, bullet.Area, Color.White * bullet.Alpha * a, 1f);
            }
        }
    }
}
