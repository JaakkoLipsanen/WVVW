using Flai;
using Flai.Graphics;
using Flai.Graphics.PostProcessing;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public class LevelRenderer : FlaiRenderer
    {
        private readonly Level _level;
        private readonly WorldRenderer _worldRenderer;
        private readonly PlayerRenderer _playerRenderer;
        private readonly GameClockRenderer _gameClockRenderer;
        private readonly StarFieldRenderer _starFieldRenderer;

        public LevelRenderer(Level level)
        {
            _level = level;
            _worldRenderer = new WorldRenderer(_level.World);
            _playerRenderer = new PlayerRenderer(_level.Player);
            _gameClockRenderer = new GameClockRenderer(_level.GameClock);
            _starFieldRenderer = new StarFieldRenderer();
        }

        protected override void LoadContentInner()
        {
            _serviceContainer.AddService<ICameraManager2D>(new CameraManager2D());
            _worldRenderer.LoadContent();
            _playerRenderer.LoadContent();
            _gameClockRenderer.LoadContent();
            _starFieldRenderer.LoadContent();
        }

        protected override void UnloadInner()
        {
            _playerRenderer.Unload();
            _worldRenderer.Unload();
            _gameClockRenderer.Unload();
            _starFieldRenderer.Unload();

            _serviceContainer.RemoveService<ICameraManager2D>();
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _playerRenderer.Update(updateContext);
            _worldRenderer.Update(updateContext);
            _gameClockRenderer.Update(updateContext);
            _starFieldRenderer.Update(updateContext);
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            graphicsContext.GraphicsDevice.Clear(FlaiGame.Current.ClearColor);

            ICamera2D camera = graphicsContext.Services.GetService<ICameraManager2D>().ActiveCamera;

            // Draw stars
            _starFieldRenderer.Draw(graphicsContext);

            // Draw the scene
            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp, camera.GetTransform(graphicsContext.GraphicsDevice));
            
            _worldRenderer.DrawBackground(graphicsContext);
            _playerRenderer.Draw(graphicsContext);
            _worldRenderer.Draw(graphicsContext);

            graphicsContext.SpriteBatch.End();

            // Draw without camera
            _gameClockRenderer.DrawOther(graphicsContext);
            _playerRenderer.DrawOther(graphicsContext);
        }
    }
}
