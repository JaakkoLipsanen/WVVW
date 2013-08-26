using System;
using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using WVVW.Screens;

namespace WVVW
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class WvvwGame : FlaiGame
    {
        public WvvwGame()
        {
            Cursor.IsVisible = true;
            base.ClearColor = new Color(32, 32, 32);

            base.InactiveSleepTime = TimeSpan.Zero;
            base.WindowTitle = "WVVW";
            _contentProvider.RootDirectory = "WvVW.Content";
            base.Content.RootDirectory = "WvVW.Content";
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _screenManager.Update(updateContext);
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            _screenManager.Draw(graphicsContext);
        }

        protected override void AddInitialScreens()
        {
            _screenManager.AddScreen(new MainMenuScreen());
        }

        protected override void InitializeGraphicsSettings()
        {
            _graphicsDeviceManager.PreferredBackBufferWidth = WvvwGlobals.ScreenWidth;
            _graphicsDeviceManager.PreferredBackBufferHeight = WvvwGlobals.ScreenHeight;
            _graphicsDeviceManager.PreferMultiSampling = true;
            _graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        }
    }

    #region Entry Point

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (WvvwGame game = new WvvwGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
