using Flai;
using Flai.ScreenManagement.Screens;
using Microsoft.Xna.Framework.Input;

namespace WVVW.Screens
{
    public class PauseScreen : TextOverlayScreen
    {
        private readonly string _levelPath;
        public PauseScreen(string levelPath)
            : base("PAUSED", "Press space to continue or Esc to exit")
        {
            _levelPath = levelPath;
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (updateContext.InputState.IsNewKeyPress(Keys.Space))
            {
                this.ExitScreen();
            }
            else if (updateContext.InputState.IsNewKeyPress(Keys.Escape))
            {
                LoadingScreen.Load(base.ScreenManager, false, new LevelSelectScreen(_levelPath));
            }
        }
    }
}
