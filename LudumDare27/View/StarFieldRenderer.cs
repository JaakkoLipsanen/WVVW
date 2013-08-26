using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Flai;
using Flai.DataStructures;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WVVW.View
{
    public class StarFieldRenderer : FlaiRenderer
    {
        // Even though star field is at least in theory part of the model,
        // there is no reason why the actual object should belong to the world or
        // any other part of the model since it is only a graphical effect and has no effect
        // on anything other than its self.
        private StarField _starField;

        public StarFieldRenderer()
        {
        }

        protected override void LoadContentInner()
        {
            _starField = new StarField(new Vector2i(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height));
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _starField.Update(updateContext);
        }

        // todo: allow zooming? it could be done so that the star-area is repeated
        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            ICamera2D camera = graphicsContext.Services.GetService<ICameraManager2D>().ActiveCamera;
            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp);
            foreach (Star star in _starField.Stars)
            {
                Vector2 starPosition = star.Position + camera.Position / 16f;
                starPosition.X = FlaiMath.RealModulus(starPosition.X, graphicsContext.ScreenSize.X);
                starPosition.Y = FlaiMath.RealModulus(starPosition.Y, graphicsContext.ScreenSize.Y);

                Color finalColor = Color.Lerp(Color.DimGray, Color.White, star.Brightness);
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, starPosition, finalColor, 0, star.Size);

                // Check if the star is partly off the screen (and thus needs to be drawn twice)
                float newX = float.MinValue;
                if (starPosition.X > graphicsContext.ViewportSize.X - star.Size)
                {
                    newX = starPosition.X - graphicsContext.ViewportSize.X;
                }

                float newY = float.MinValue;
                if (starPosition.Y > graphicsContext.ViewportSize.Y - star.Size)
                {
                    newY = starPosition.Y - graphicsContext.ViewportSize.Y;
                }

                if (newX != float.MinValue || newY != float.MinValue)
                {
                    Vector2 newPosition = new Vector2(
                        newX == float.MinValue ? starPosition.X : newX,
                        newY == float.MinValue ? starPosition.Y : newY);

                    graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, newPosition, finalColor, 0, star.Size);
                }
            }
            graphicsContext.SpriteBatch.End();
        }

        #region Star and StarField

        // TODO: Add some color change? For example that some stars can be a bit reddish, some bluish etc
        private class Star : IComparable<Star>
        {
            private static readonly Vector2 AxisSpeedMultiplier = new Vector2(150, 2f);
            public const int BaseSize = 12;

            private readonly Vector2 _startingPosition;
            private readonly float _scale;
            private readonly float _distance;
            private readonly float _brightness;

            public float Size
            {
                get { return _distance * _scale * Star.BaseSize; }
            }

            public float Brightness
            {
                get { return _brightness * _scale; }
            }

            public Vector2 Position { get; private set; }

            public Star(Vector2 defaultPosition, float scale, float distance, float brightness)
            {
                _scale = scale;
                _distance = distance;
                _brightness = brightness;

                _startingPosition = defaultPosition;
            }

            public void Update(float totalMovementTime, Vector2i wrappingAreaSize)
            {
                this.Position = new Vector2(
                    FlaiMath.RealModulus(_startingPosition.X - totalMovementTime * _distance * Star.AxisSpeedMultiplier.X, wrappingAreaSize.X),
                    _startingPosition.Y);// FlaiMath.RealModulus(_startingPosition.Y - totalMovementTime * _distance * Star.AxisSpeedMultiplier.Y, wrappingAreaSize.Y));
            }

            public int CompareTo(Star other)
            {
                return _distance.CompareTo(other._distance);
            }
        }

        // planets etc?
        private class StarField
        {
            private const float Density = 0.45f;

            private readonly Star[] _stars;
            private readonly ReadOnlyArray<Star> _readOnlyStars;

            private readonly Vector2i _wrappingAreaSize;
            private float _totalTime = 0f;

            public ReadOnlyArray<Star> Stars
            {
                get { return _readOnlyStars; }
            }

            public StarField(Vector2i wrappingAreaSize)
                : this(wrappingAreaSize, Environment.TickCount)
            {
            }

            public StarField(Vector2i wrappingAreaSize, int seed)
            {
                _wrappingAreaSize = wrappingAreaSize;
                _stars = this.GenerateStars(seed);
                _readOnlyStars = new ReadOnlyArray<Star>(_stars);
                foreach (Star star in _stars)
                {
                    star.Update(0f, _wrappingAreaSize);
                }
            }

            public void Update(UpdateContext updateContext)
            {
                _totalTime += updateContext.DeltaSeconds;
                foreach (Star star in _stars)
                {
                    star.Update(_totalTime, _wrappingAreaSize);
                }
            }

            private Star[] GenerateStars(int seed)
            {
                int gridWidth = (int)(_wrappingAreaSize.X / (Star.BaseSize * 4) * Density);
                int gridHeight = (int)(_wrappingAreaSize.Y / (Star.BaseSize * 4) * Density);

                float cellWidth = _wrappingAreaSize.X / (float)gridWidth;
                float cellHeight = _wrappingAreaSize.Y / (float)gridHeight;

                FlaiRandom random = new FlaiRandom(seed);
                Star[] stars = new Star[gridWidth * gridHeight];
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        Vector2 position = new Vector2(
                            random.NextFloat(cellWidth * x, cellWidth * (x + 1)),
                            random.NextFloat(cellHeight * y, cellHeight * (y + 1)));

                        float scale = random.NextFloat(0.35f, 0.65f);
                        float brightness = random.NextFloat(0.1f, 0.45f);
                        float distance = random.NextFloat(0.2f, 0.95f);

                        stars[x + y * gridWidth] = new Star(position, scale, distance, brightness);
                    }
                }

                // sort the stars based on their distance
                // so that they are automatically drawn
                // from back to front
                Array.Sort(stars);
                return stars;
            }
        }
        #endregion
    }
}
