using Flai.Graphics;
using Flai.ScreenManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WVVW.Screens
{
    public class TextOverlayScreen : GameScreen
    {
        private readonly string _title;
        private readonly string _content;

        public override bool IsPopup
        {
            get { return true; }
        }

        public TextOverlayScreen(string title, string content)
        {
            _title = title;
            _content = content;
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            const string TitleFontName = "Minecraftia40";
            const string ContentFontName = "Minecraftia18";

            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp);
            graphicsContext.SpriteBatch.DrawFullscreen(graphicsContext.BlankTexture, Color.Black * 0.5f);

            if(_title != null)
            {
                graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[TitleFontName], _title, new Vector2(graphicsContext.ScreenSize.X / 2f, 96), Color.White);
            }

            if (_content != null)
            {
                graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[ContentFontName], _content, new Vector2(graphicsContext.ScreenSize.X / 2f, 144), Color.White);
            }

            graphicsContext.SpriteBatch.End();
        }
    }
}
