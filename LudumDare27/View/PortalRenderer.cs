using System.Collections.Generic;
using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using WVVW.Model;

namespace WVVW.View
{
    public class PortalRenderer : FlaiRenderer
    {
        private readonly World _world;
        
        private const int ParticleCount = 60;
        private const float TimeBetweenRects = PortalParticle.Life / PortalRenderer.ParticleCount;
        private const float DirectionStep = 0.6f;

        private readonly List<PortalParticle> _portalParticles = new List<PortalParticle>();

        private float _timeSinceLastParticle = 0f;
        private float _nextDirection = 0f;

        public PortalRenderer(World world)
        {
            _world = world;

            const float InitialUpdateDelta = 0.016f;
            for (int i = 0; i < PortalParticle.Life / InitialUpdateDelta; i++)
            {
                for (int j = 0; j < _portalParticles.Count; j++)
                {
                    _portalParticles[j].Update(InitialUpdateDelta);
                }
                _portalParticles.RemoveAll(particle => particle.Alpha <= 0);

                _timeSinceLastParticle += InitialUpdateDelta;
                while (_timeSinceLastParticle > PortalRenderer.TimeBetweenRects)
                {
                    _timeSinceLastParticle -= PortalRenderer.TimeBetweenRects;
                    _portalParticles.Add(new PortalParticle(_world.Portal.Area.Center, Vector2.Normalize(FlaiMath.GetAngleVector(_nextDirection))));
                    _nextDirection += PortalRenderer.DirectionStep;
                }
            }
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            for (int i = 0; i < _portalParticles.Count; i++)
            {
                _portalParticles[i].Update(updateContext.DeltaSeconds);
            }
            _portalParticles.RemoveAll(particle => particle.Alpha <= 0);

            _timeSinceLastParticle += updateContext.DeltaSeconds;
            while (_timeSinceLastParticle > PortalRenderer.TimeBetweenRects)
            {
                _timeSinceLastParticle -= PortalRenderer.TimeBetweenRects;
                _portalParticles.Add(new PortalParticle(_world.Portal.Area.Center, Vector2.Normalize(FlaiMath.GetAngleVector(_nextDirection))));
                _nextDirection += PortalRenderer.DirectionStep;
            }
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
          /* if (_portal != null)
            {
                graphicsContext.PrimitiveRenderer.DrawRectangle(graphicsContext, _portal.Area, Color.White);
                graphicsContext.PrimitiveRenderer.DrawRectangleOutlines(graphicsContext, _portal.Area, Color.Black);
            } */

            for (int i = 0; i < _portalParticles.Count; i++)
            {
                PortalParticle portalParticle = _portalParticles[i];

                // Black
                graphicsContext.SpriteBatch.DrawCentered(graphicsContext.BlankTexture, portalParticle.Position, 
                    Color.Black * (1 - portalParticle.Alpha * portalParticle.Alpha * portalParticle.Alpha * portalParticle.Alpha), 0f, (0.2f + portalParticle.Scale) * Tile.Size);

                // White
                graphicsContext.SpriteBatch.DrawCentered(graphicsContext.BlankTexture, portalParticle.Position, 
                    Color.White * (1 - portalParticle.Alpha * portalParticle.Alpha * portalParticle.Alpha * portalParticle.Alpha), 0f, (0.2f + portalParticle.Scale) * (Tile.Size - 4));
            }
        }

        #region Portal Particle

        // Make this a struct or make an object pool for goal particles
        private class PortalParticle
        {
            public const float Life = 4f;

            private readonly Vector2 _initialPosition;
            private readonly Vector2 _targetPosition;
            private float _currentLife = 0f;

            public float Alpha
            {
                get { return (PortalParticle.Life - _currentLife) / PortalParticle.Life; }
            }

            public float Scale
            {
                get { return this.Alpha; }
            }

            public Vector2 Position
            {
                get { return Vector2.Lerp(_initialPosition, _targetPosition, this.Alpha); }
            }

            public PortalParticle(Vector2 position, Vector2 direction)
            {
                _initialPosition = position;
                _targetPosition = _initialPosition + direction * 24f;
            }

            public void Update(float delta)
            {
                _currentLife += delta;
            }
        }

        #endregion
    }
}
