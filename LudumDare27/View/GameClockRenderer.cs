using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public class GameClockRenderer : FlaiRenderer
    {
        private readonly GameClock _gameClock;
        public GameClockRenderer(GameClock gameClock)
        {
            _gameClock = gameClock;
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {            
        }

        // Draw without camera
        public void DrawOther(GraphicsContext graphicsContext)
        {
            const string FontName = "Minecraftia40";
            Color color = _gameClock.RemainingTime > 2f ? new Color(224, 224, 224) : new Color(255, 64, 64);
            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp);
            graphicsContext.SpriteBatch.DrawStringFadedCentered(graphicsContext.FontContainer[FontName], _gameClock.RemainingTimeString, new Vector2(graphicsContext.ScreenSize.X / 2f, 48), Color.Black, color);
            graphicsContext.SpriteBatch.End();
        }
    }
}
