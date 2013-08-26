using Flai;
using Flai.Graphics;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WVVW.Screens
{
    public class MainMenuScreen : GameScreen
    {
        private readonly BasicUiContainer _uiContainer = new BasicUiContainer();
        public MainMenuScreen()
        {
        }

        protected override void LoadContent()
        {
            base.Game.ChangeResolution(WvvwGlobals.ScreenWidth, WvvwGlobals.ScreenHeight);
            this.CreateUi();
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (this.IsActive)
            {
                if (updateContext.InputState.IsNewKeyPress(Keys.Escape))
                {
                    base.Game.Exit();
                    return;
                }

                _uiContainer.Update(updateContext);
            }
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp);
            _uiContainer.Draw(graphicsContext, true);

            const string FontName = "Minecraftia18";
            graphicsContext.SpriteBatch.DrawString(graphicsContext.FontContainer[FontName], "Jaakko Lipsanen/Flai", new Vector2(4, graphicsContext.ScreenSize.Y - 40), Color.White);
            graphicsContext.SpriteBatch.End();
        }

        private void CreateUi()
        {
            const string FontName = "Minecraftia32"; 
            TextButton playButton = new TextButton("Play", new Vector2(base.Game.WindowSize.X / 2f, base.Game.WindowSize.Y / 4f + 48)) { Font = FontName };
            playButton.Click += (o, e) =>
            {
                LoadingScreen.Load(base.ScreenManager, false, new LevelSelectScreen(null));
            };
            _uiContainer.Add(playButton);

            TextButton editButton = new TextButton("Edit", new Vector2(base.Game.WindowSize.X / 2f, base.Game.WindowSize.Y / 2f)) { Font = FontName };
            editButton.Click += (o, e) =>
            {
                LoadingScreen.Load(base.ScreenManager, false, new EditorScreen());
            };
            _uiContainer.Add(editButton);

            TextButton exitButton = new TextButton("Exit", new Vector2(base.Game.WindowSize.X / 2f, base.Game.WindowSize.Y / 4f * 3f - 48)) { Font = FontName };
            exitButton.Click += (o, e) =>
            {
                base.Game.Exit();
            };
            _uiContainer.Add(exitButton);
        }
    }
}
