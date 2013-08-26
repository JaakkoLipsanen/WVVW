using Flai;
using Flai.Graphics;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Editor.Model;
using WVVW.Model;

namespace WVVW.Editor.Ui
{
    public class ResizeButton : ButtonBase
    {
        private const int Size = 40;
        private static readonly RectangleF Area = new RectangleF(0, EditorGlobals.EditorScreenHeight - ResizeButton.Size, ResizeButton.Size, ResizeButton.Size);

        private readonly EditorPlayer _player;
        public ResizeButton(EditorPlayer player)
            : base(ResizeButton.Area)
        {
            _player = player;
        }

        public override void Draw(GraphicsContext graphicsContext)
        {
            Texture2D texture = graphicsContext.ContentProvider.DefaultManager.LoadTexture("ResizeIcon");

            bool isToggled = _player.EditorMode == EditorMode.Resizing;
            graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, _area, isToggled ? Color.White : Color.DimGray * 0.75f);
            graphicsContext.SpriteBatch.Draw(texture, _area, isToggled ? Color.DimGray : Color.White);
        }

        protected override void OnClick()
        {
            if (_player.EditorMode == EditorMode.Normal)
            {
                _player.EditorMode = EditorMode.Resizing;
            }
            else if (_player.EditorMode == EditorMode.Resizing)
            {
                _player.EditorMode = EditorMode.Normal;
            }
        }
    }
}
