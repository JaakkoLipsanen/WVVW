using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public static class TileRenderer
    {
        public static void DrawTile(GraphicsContext graphicsContext, Vector2 position, TileType tile)
        {
            if (tile == TileType.Air)
            {
                return;
            }

            RectangleF area = new RectangleF(position.X, position.Y, Tile.Size, Tile.Size);
            Vector2 center = position + Vector2.One * 0.5f * Tile.Size;

            if (tile == TileType.Solid)
            {
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, area);
            }
            else if (tile == TileType.SpikeDown || tile == TileType.SpikeLeft || tile == TileType.SpikeRight || tile == TileType.SpikeUp)
            {
                Texture2D spikeTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Spike");

                Vector2 offset = Vector2.Zero;
                switch (tile)
                {
                    case TileType.SpikeUp: offset = new Vector2(0, 1); break;
                    case TileType.SpikeRight: offset = new Vector2(-1, 0); break;
                    case TileType.SpikeDown: offset = new Vector2(0, -1); break;
                    case TileType.SpikeLeft: offset = new Vector2(1, 0); break;
                }

                //
                float rotation = ((int)tile - (int)TileType.SpikeUp) * FlaiMath.PiOver2;
                graphicsContext.SpriteBatch.DrawCentered(spikeTexture, center + offset, Color.White, rotation, 1);
            }
            else if (Tile.IsClock(tile))
            {
                const string FontName = "Minecraftia12";
                string c = tile == TileType.Clock5 ? "5" : (tile == TileType.Clock10 ? "10" : "15");

                graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[FontName], c, center, Color.White);
                // Texture2D clockTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Clock");
                // graphicsContext.SpriteBatch.DrawCentered(clockTexture, center, Color.White, 0, Vector2.One * 2);
            }
            else if (tile == TileType.DoorLock)
            {
                Texture2D lockTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Lock");
                graphicsContext.SpriteBatch.DrawCentered(lockTexture, center, Color.White, 0, 4);
            }
            else if (Tile.IsCannon(tile))
            {
                // ignore
            }
            else
            {
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, area, Color.Purple);
            }
        }
    }
}
