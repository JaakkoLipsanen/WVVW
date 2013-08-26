using Flai;
using Flai.Graphics;
using Flai.Graphics.PostProcessing;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Editor.Model;

namespace WVVW.Editor.View
{
    public class EditorLevelRenderer : FlaiRenderer
    { 
        private readonly EditorLevel _level;
        private readonly EditorWorldRenderer _worldRenderer;
        private readonly EditorPlayerRenderer _playerRenderer;
        private readonly PostProcessRenderer _postProcessRenderer;

        public EditorLevelRenderer(EditorLevel level)
        {
            _level = level;
            _worldRenderer = new EditorWorldRenderer(_level.World);
            _playerRenderer = new EditorPlayerRenderer(_level.World, _level.Player);
            _postProcessRenderer = new PostProcessRenderer(4);
        }

        protected override void LoadContentInner()
        {
            _serviceContainer.AddService<ICameraManager2D>(new CameraManager2D());

            _worldRenderer.LoadContent();
            _playerRenderer.LoadContent();

            _postProcessRenderer.LoadContent();
        }

        protected override void UnloadInner()
        {
            _playerRenderer.Unload();
            _worldRenderer.Unload();

            _serviceContainer.RemoveService<ICameraManager2D>();
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _playerRenderer.Update(updateContext);
            _worldRenderer.Update(updateContext);
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            _postProcessRenderer.BeginDraw(graphicsContext);
            graphicsContext.GraphicsDevice.Clear(FlaiGame.Current.ClearColor);

            ICamera2D camera = graphicsContext.Services.GetService<ICameraManager2D>().ActiveCamera;
            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp, camera.GetTransform(graphicsContext.GraphicsDevice));

            // Draw the scene
            _worldRenderer.Draw(graphicsContext);
            _playerRenderer.Draw(graphicsContext);

            graphicsContext.SpriteBatch.End();

            // Draw post-processes
            _postProcessRenderer.Draw(graphicsContext);
        }
    }
}
