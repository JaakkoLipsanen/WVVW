using Flai;
using Microsoft.Xna.Framework;

namespace WVVW.Model
{
    public class Portal
    {
        private static readonly Vector2 Size = new Vector2(Tile.Size * 2.1f, Tile.Size * 2.1f);

        private readonly Vector2i _index;
        public RectangleF Area
        {
            get { return new RectangleF(_index.X * Tile.Size - Portal.Size.X / 2f, _index.Y * Tile.Size - Portal.Size.Y / 2f, Portal.Size.X, Portal.Size.Y); }
        }

        public Portal(Vector2i index)
        {
            _index = index;
        }
    }
}
