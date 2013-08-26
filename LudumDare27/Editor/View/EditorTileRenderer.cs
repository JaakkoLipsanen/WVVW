using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.Editor.View
{
    public static class EditorTileRenderer
    {
        public static void DrawTile(GraphicsContext graphicsContext, Vector2 position, TileType type)
        {
            EditorTileRenderer.DrawTile(graphicsContext, position, type, Color.White, Vector2.One);
        }

        // !! This is not very good if the tiles can move (for example like in SpaceBucks)
        public static void DrawTile(GraphicsContext graphicsContext, Vector2 position, TileType tile, Color colorOverlay, Vector2 scale)
        {
            if (tile == TileType.Air)
            {
                return;
            }

            RectangleF area = new RectangleF(position.X, position.Y, Tile.Size * scale.X, Tile.Size * scale.Y);
            Vector2 center = position + Vector2.One * 0.5f * Tile.Size * scale;

            if (tile == TileType.Solid)
            {
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, area, colorOverlay);
            }
            else if (Tile.IsSpike(tile))
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
                graphicsContext.SpriteBatch.DrawCentered(spikeTexture, center + offset, colorOverlay, rotation, Vector2.One);
            }
            else if (tile == TileType.PlayerSpawn || tile == TileType.PlayerGoal)
            {
                const string FontName = "Minecraftia12";
                char c = (tile == TileType.PlayerSpawn) ? 'S' : 'G';
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, area, colorOverlay);
                graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[FontName], c, center, Color.Black, 0, scale);
            }
            else if(Tile.IsClock(tile))
            {
                const string FontName = "Minecraftia12";
                string c = tile == TileType.Clock5 ? "5" : (tile == TileType.Clock10 ? "10" : "15");

                graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[FontName], c, center, colorOverlay, 0, Vector2.One * ((scale == Vector2.One) ? 1 : 1.5f));
            }
            else if (Tile.IsSpring(tile))
            {
                float rotation = (tile == TileType.SpringHorizontal) ? -FlaiMath.PiOver2 : 0;
                Vector2 lineStartPosition = (tile == TileType.SpringHorizontal) ? new Vector2(area.Left, area.Center.Y) : new Vector2(area.Center.X, area.Top);
                graphicsContext.PrimitiveRenderer.DrawLine(graphicsContext, lineStartPosition, colorOverlay, rotation, Tile.Size * scale.X, Spring.Thickness);
            }
            else if (tile == TileType.Key)
            {
                Texture2D keyTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Key");
                graphicsContext.SpriteBatch.DrawCentered(keyTexture, center, colorOverlay, 0, scale * 2);
            }
            else if (tile == TileType.DoorLock)
            {
                Texture2D lockTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Lock");
                graphicsContext.SpriteBatch.DrawCentered(lockTexture, center, colorOverlay, 0, (Tile.Size / (float)lockTexture.Width) * scale);
            }
            else if (Tile.IsCannon(tile))
            {
                Texture2D cannonTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Cannon");
                float rotation = ((int)tile - (int)TileType.CannonUp) * FlaiMath.PiOver2;
                graphicsContext.SpriteBatch.DrawCentered(cannonTexture, center, Color.White, rotation, (scale == Vector2.One) ? 2 : 2);
            }
            else if (Tile.IsStage(tile))
            {
                const string FontName = "Minecraftia12";
                string s = "S" + ((int)tile - (int)TileType.Stage1 + 1).ToString();
                SpriteFont font = graphicsContext.FontContainer[FontName];
                Vector2 baseScale = new Vector2(Tile.Size) / font.MeasureString(s);
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, area, colorOverlay);
                graphicsContext.SpriteBatch.DrawStringCentered(font, s, center, Color.Black, 0, baseScale * scale);
            }
        }
    }
}