using Flai;
using Flai.Input;
using WVVW.Model;

namespace WVVW.Controller
{
    public class LevelController : FlaiController
    {
        private readonly Level _level;
        private readonly PlayerController _playerController;

        public LevelController(Level level)
        {
            _level = level;
            _playerController = new PlayerController(_level.Player);
        }

        public override void Control(UpdateContext updateContext)
        {
            _playerController.Control(updateContext);
        }
    }
}
