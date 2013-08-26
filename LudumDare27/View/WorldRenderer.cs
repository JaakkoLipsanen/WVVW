using Flai;
using Flai.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public class WorldRenderer : FlaiRenderer
    {
        private readonly World _world;
        private readonly WorldMapRenderer _mapRenderer;
        private readonly TimeBonusRenderer _timeBonusRenderer;
        private readonly SpringRenderer _springRenderer;
        private readonly KeyRenderer _keyRenderer;
        private readonly CannonRenderer _cannonRenderer;
        private readonly DoorRenderer _doorRenderer;
        private readonly PortalRenderer _portalRenderer;

        public WorldRenderer(World world)
        {
            _world = world;
            _mapRenderer = new WorldMapRenderer(_world.Map);
            _timeBonusRenderer = new TimeBonusRenderer(_world.TimeBonuses);
            _springRenderer = new SpringRenderer(_world.Springs);
            _keyRenderer = new KeyRenderer(_world.Keys);
            _cannonRenderer = new CannonRenderer(_world.Cannons);
            _doorRenderer = new DoorRenderer(_world.Doors);
            _portalRenderer = new PortalRenderer(_world);
        }

        protected override void LoadContentInner()
        {
            _mapRenderer.LoadContent();
            _timeBonusRenderer.LoadContent();
            _springRenderer.LoadContent();
            _keyRenderer.LoadContent();
            _cannonRenderer.LoadContent();
            _doorRenderer.LoadContent();
            _portalRenderer.LoadContent();
        }

        protected override void UnloadInner()
        {
            _mapRenderer.Unload();
            _timeBonusRenderer.Unload();
            _springRenderer.Unload();
            _keyRenderer.Unload();
            _cannonRenderer.Unload();
            _doorRenderer.Unload();
            _portalRenderer.Unload();
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _mapRenderer.Update(updateContext);
            _timeBonusRenderer.Update(updateContext);
            _springRenderer.Update(updateContext);
            _keyRenderer.Update(updateContext);
            _cannonRenderer.Update(updateContext);
            _doorRenderer.Update(updateContext);
            _portalRenderer.Update(updateContext);
        }

        public void DrawBackground(GraphicsContext graphicsContext)
        {
            _springRenderer.Draw(graphicsContext);
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            _mapRenderer.Draw(graphicsContext);
            _doorRenderer.Draw(graphicsContext);
            _cannonRenderer.Draw(graphicsContext);
            _timeBonusRenderer.Draw(graphicsContext);
            _keyRenderer.Draw(graphicsContext);
            _portalRenderer.Draw(graphicsContext);
        }
    }
}