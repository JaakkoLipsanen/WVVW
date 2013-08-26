using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using WVVW.Editor.Model;
using WVVW.Model;

namespace WVVW.Editor.View
{
    public class EditorPlayerRenderer : FlaiRenderer
    {
        private readonly EditorWorld _world;
        private readonly EditorPlayer _player;

        public EditorPlayerRenderer(EditorWorld world, EditorPlayer player)
        {
            _world = world;
            _player = player;
        }

        protected override void LoadContentInner()
        {
            _serviceContainer.GetService<ICameraManager2D>().AddCamera("Editor", _player.Camera);
        }

        protected override void UnloadInner()
        {
            _serviceContainer.GetService<ICameraManager2D>().RemoveCamera("Editor");
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            if (_player.EditorMode == EditorMode.Normal)
            {
                if (_player.MouseIndex.HasValue)
                {
                    EditorTileRenderer.DrawTile(graphicsContext, _player.MouseIndex.Value * Tile.Size, _player.CurrentTileType, Color.White * 0.5f, Vector2.One);
                }
            }
            else if (_player.EditorMode == EditorMode.Resizing)
            {
                Vector2i roomSize = new Vector2i(Room.Width * Tile.Size, Room.Height * Tile.Size);
                graphicsContext.PrimitiveRenderer.DrawRectangleOutlines(graphicsContext, new Rectangle(-roomSize.X, -roomSize.Y, (_world.Map.WidthInRooms + 2) * roomSize.X, (_world.Map.HeightInRooms + 2) * roomSize.Y), Color.RoyalBlue * 0.75f, 2f / _player.Camera.Zoom);
            }
        }
    }
}
