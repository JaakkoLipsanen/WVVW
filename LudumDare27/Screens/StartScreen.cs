using Flai;
using Microsoft.Xna.Framework.Input;

namespace WVVW.Screens
{
    public class StartScreen : TextOverlayScreen
    {
        public StartScreen()
            : base(null, "Press space to start")
        {
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (updateContext.InputState.IsNewKeyPress(Keys.Space))
            {
                this.ExitScreen();
            }
        }
    }
}
