using Flai;
using Microsoft.Xna.Framework;

namespace WVVW.Model
{
    public class Key : GameObject
    {
        private static readonly Vector2 Size = new Vector2(Tile.Size * 1.5f, Tile.Size * 1.5f);

        public event GenericEvent KeyCollected;

        private readonly Vector2i _index;
        private bool _isCollected = false;

        public Vector2 Position
        {
            get { return (_index + Vector2i.One * 0.5f) * Tile.Size; }
        }

        public override RectangleF Area
        {
            get { return new RectangleF((_index.X + 0.5f) * Tile.Size - Key.Size.X / 2f, (_index.Y + 0.5f) * Tile.Size - Key.Size.Y / 2f, Key.Size.X, Key.Size.Y); }
        }

        public override bool CollidesWithPlayer
        {
            get { return false; }
        }

        public bool IsCollected
        {
            get { return _isCollected; }
        }

        public Key(Vector2i index)
        {
            _index = index;
        }

        public void Collect()
        {
            if (_isCollected)
            {
                return;
            }

            _isCollected = true;
            this.KeyCollected.InvokeIfNotNull();
        }
    }
}
