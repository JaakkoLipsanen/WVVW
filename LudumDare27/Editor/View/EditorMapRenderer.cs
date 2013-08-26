using Flai;
using Flai.Graphics;
using Flai.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Editor.Model;
using WVVW.Model;

namespace WVVW.Editor.View
{
    public class EditorMapRenderer : FlaiRenderer
    {
        private readonly EditorMap _map;
        public EditorMapRenderer(EditorMap map)
        {
            _map = map;
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            ICamera2D camera = graphicsContext.Services.GetService<ICameraManager2D>().ActiveCamera;
            RectangleF cameraArea = camera.GetArea(graphicsContext.GraphicsDevice);

            this.DrawTiles(graphicsContext, cameraArea);
            this.DrawMapBorders(graphicsContext, camera);
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
                    EditorTileRenderer.DrawTile(graphicsContext, new Vector2(x, y) * Tile.Size, _map[x, y]);
                }
            }
        }

        #endregion

        private void DrawMapBorders(GraphicsContext graphicsContext, ICamera2D camera)
        {
            // Room borders
            const int RoomBorderThickness = 2;
            for(int x = 0; x < _map.WidthInRooms; x++)
            {
                for(int y = 0; y < _map.HeightInRooms; y++)
                {
                    Vector2i roomSize = new Vector2i(Room.Width * Tile.Size, Room.Height * Tile.Size);

                    // Draw room sub-borders
                    for (int x1 = 0; x1 < 4; x1++)
                    {
                        for (int y1 = 0; y1 < 4; y1++)
                        {
                            graphicsContext.PrimitiveRenderer.DrawRectangleOutlines(
                                graphicsContext, 
                                new RectangleF(x * roomSize.X + x1 * roomSize.X / 4f, y * roomSize.Y + y1 * roomSize.Y / 4f, roomSize.X / 4f, roomSize.Y / 4f), 
                                Color.RoyalBlue.MultiplyRGB(0.5f) * 0.5f, 
                                RoomBorderThickness / camera.Zoom);
                        }
                    }

                    // Draw room borders
                    graphicsContext.PrimitiveRenderer.DrawRectangleOutlines(graphicsContext, new Rectangle(x * roomSize.X, y * roomSize.Y, roomSize.X, roomSize.Y), Color.RoyalBlue, RoomBorderThickness / camera.Zoom);
                }
            }

            // Full map borders
            const int MapBorderThickness = 4;
            graphicsContext.PrimitiveRenderer.DrawRectangleOutlines(graphicsContext, new Rectangle(0, 0, _map.Width * Tile.Size, _map.Height * Tile.Size), Color.White, MapBorderThickness / camera.Zoom);
        }
    }
}
