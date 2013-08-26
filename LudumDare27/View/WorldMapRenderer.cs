using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using WVVW.Editor.View;
using WVVW.Model;

namespace WVVW.View
{
    public class WorldMapRenderer : FlaiRenderer
    { 
        private readonly WorldMap _map;
        public WorldMapRenderer(WorldMap map)
        {
            _map = map;
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            ICamera2D camera = graphicsContext.Services.GetService<ICameraManager2D>().ActiveCamera;
            RectangleF cameraArea = camera.GetArea(graphicsContext.GraphicsDevice);

            this.DrawTiles(graphicsContext, cameraArea);
            graphicsContext.PrimitiveRenderer.DrawLine(graphicsContext, new Vector2(0, _map.Height * Tile.Size), new Vector2(_map.Width * Tile.Size, _map.Height * Tile.Size), Color.White, 4f);
        }

        #region Draw Tiles

        private void DrawTiles(GraphicsContext graphicsContext, RectangleF cameraArea)
        {
            int left = FlaiMath.Max(0, Tile.WorldToTileCoordinate(cameraArea.Left));
            int right = FlaiMath.Min(_map.Width - 1, Tile.WorldToTileCoordinate(cameraArea.Right) + 1);
            int top = FlaiMath.Max(0, Tile.WorldToTileCoordinate(cameraArea.Top));
            int bottom = FlaiMath.Min(_map.Height - 1, Tile.WorldToTileCoordinate(cameraArea.Bottom) + 1);

            for (int y = top; y <= bottom; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    TileRenderer.DrawTile(graphicsContext, new Vector2(x, y) * Tile.Size, _map[x, y]);
                }
            }
        }

        #endregion
    }
}
