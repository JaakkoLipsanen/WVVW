using System.Collections.ObjectModel;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public class DoorRenderer : FlaiRenderer
    {
        private readonly ReadOnlyCollection<Door> _doors;
        public DoorRenderer(ReadOnlyCollection<Door> doors)
        {
            _doors = doors;
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            foreach (Door door in _doors)
            {
                Texture2D lockTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Lock");
                graphicsContext.SpriteBatch.DrawCentered(lockTexture, door.Area.Center, Color.White * door.Alpha, 0, (Tile.Size / (float)lockTexture.Width) + 2 * (1 - door.Alpha));
            }
        }
    }
}
