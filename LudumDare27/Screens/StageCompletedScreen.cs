using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flai;
using Flai.ScreenManagement.Screens;
using Microsoft.Xna.Framework.Input;

namespace WVVW.Screens
{
    public class StageCompletedScreen : TextOverlayScreen
    {
        private readonly string _levelPath;
        public StageCompletedScreen(string levelPath, bool fullGameCompleted)
            : base(fullGameCompleted ? "Level Completed!" : "Stage Completed", "Press esc or space to return")
        {
            _levelPath = levelPath;
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (this.IsActive)
            {
                if (updateContext.InputState.IsNewKeyPress(Keys.Escape) || updateContext.InputState.IsNewKeyPress(Keys.Space))
                {
                    LoadingScreen.Load(base.ScreenManager, false, new LevelSelectScreen(_levelPath));
                }
            }
        }
    }
}
