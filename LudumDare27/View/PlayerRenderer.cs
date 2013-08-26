using Flai;
using Flai.Graphics;
using Flai.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public class PlayerRenderer : FlaiRenderer
    {
        private readonly Player _player;
        private readonly Camera2D _camera;

        public PlayerRenderer(Player player)
        {
            _player = player;
            _camera = new Camera2D((_player.RoomIndex + Vector2i.One * 0.5f) * Room.Size * Tile.Size);
        }

        protected override void LoadContentInner()
        {
            _serviceContainer.GetService<ICameraManager2D>().AddCamera("Player", _camera);
        }

        protected override void UnloadInner()
        {
            _serviceContainer.GetService<ICameraManager2D>().RemoveCamera("Player");
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _camera.Position = (_player.RoomIndex + Vector2i.One * 0.5f) * Room.Size * Tile.Size;
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            RectangleF area = _player.Area.Inflate(1, 1);
            if (_player.IsAlive)
            {
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, area, Color.White);
            }
            else
            {
                const float FlashTime = 0.15f;
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, area, graphicsContext.TotalSeconds % FlashTime * 2 > FlashTime ? new Color(255, 64, 64) : Color.Transparent);
            }
        }

        public void DrawOther(GraphicsContext graphicsContext)
        {
            // Draw key-count
            const string KeyFontName = "Minecraftia18";
            Color color = new Color(204, 204, 204);
            Texture2D keyTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Key");
            
            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp);

            // Draw key texture shaded
            graphicsContext.SpriteBatch.DrawCentered(keyTexture, new Vector2(graphicsContext.ScreenSize.X - 48 - 1, 2 + 18 - 1), Color.Black, 0, 4);
            graphicsContext.SpriteBatch.DrawCentered(keyTexture, new Vector2(graphicsContext.ScreenSize.X - 48, 2 + 18), color.MultiplyRGB(1), 0, 4);

            // Draw key-count
            graphicsContext.SpriteBatch.DrawStringFaded(graphicsContext.FontContainer[KeyFontName], _player.Keys.ToString(), new Vector2(graphicsContext.ScreenSize.X - 24, 2), Color.Black, color);

            graphicsContext.SpriteBatch.End();
        }
    }
}
