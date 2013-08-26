using System.Collections.ObjectModel;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Model;

namespace WVVW.View
{
    public  class KeyRenderer : FlaiRenderer
    {
        private readonly ReadOnlyCollection<Key> _keys;
        public KeyRenderer(ReadOnlyCollection<Key> keys)
        {
            _keys = keys;
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            foreach (Key key in _keys)
            {
                if (!key.IsCollected)
                {
                    Texture2D keyTexture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("Key");
                    graphicsContext.SpriteBatch.DrawCentered(keyTexture, key.Position, Color.White, 0, 2);
                }
            }
        }
    }
}
