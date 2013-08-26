using Flai;
using Flai.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WVVW.Model;

namespace WVVW.Controller
{
    public class PlayerController : FlaiController
    {
        private readonly Player _player;
        public PlayerController(Player player)
        {
            _player = player;
        }

        public override void Control(UpdateContext updateContext)
        {
            if (_player.IsAlive)
            {
                this.HandleMovement(updateContext);
                this.HandleJumping(updateContext);
            }
        }

        private void HandleMovement(UpdateContext updateContext)
        {
            float horizontalMovement = 0f;
            if (updateContext.InputState.IsKeyPressed(Keys.A) || updateContext.InputState.IsKeyPressed(Keys.Left))
            {
                horizontalMovement -= Player.Speed;
            }

            if (updateContext.InputState.IsKeyPressed(Keys.D) || updateContext.InputState.IsKeyPressed(Keys.Right))
            {
                horizontalMovement += Player.Speed;
            }

            if (horizontalMovement != 0)
            {
                _player.HorizontalVelocity = horizontalMovement * updateContext.DeltaSeconds;
            }
        }

        private void HandleJumping(UpdateContext updateContext)
        {
            // TODO: Pre/Late jumping?
            if (updateContext.InputState.IsNewKeyPress(Keys.Space))
            {
                _player.Jump();
            }
        }
    }
}
