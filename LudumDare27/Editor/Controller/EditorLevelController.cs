using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flai;
using Flai.Input;
using WVVW.Editor.Model;

namespace WVVW.Editor.Controller
{
    public class EditorLevelController : FlaiController
    {  
        private readonly EditorLevel _level;
        private readonly EditorPlayerController _playerController;

        public EditorLevelController(EditorLevel level)
        {
            _level = level;
            _playerController = new EditorPlayerController(_level.World, _level.Player);
        }

        public override void Control(UpdateContext updateContext)
        {
            _playerController.Control(updateContext);
        }
    }
}
