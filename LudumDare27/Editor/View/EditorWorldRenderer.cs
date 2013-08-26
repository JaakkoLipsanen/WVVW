using Flai;
using Flai.Graphics;
using WVVW.Editor.Model;

namespace WVVW.Editor.View
{
    public class EditorWorldRenderer : FlaiRenderer
    { 
        private readonly EditorWorld _world;
        private readonly EditorMapRenderer _mapRenderer;

        public EditorWorldRenderer(EditorWorld world)
        {
            _world = world;
            _mapRenderer = new EditorMapRenderer(_world.Map);
        }

        protected override void LoadContentInner()
        {
            _mapRenderer.LoadContent();
        }

        protected override void UnloadInner()
        {
            _mapRenderer.Unload();
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _mapRenderer.Update(updateContext);
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            _mapRenderer.Draw(graphicsContext);
        }
    }
}
