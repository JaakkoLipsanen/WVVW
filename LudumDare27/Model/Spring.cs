using Flai;
using Microsoft.Xna.Framework;

namespace WVVW.Model
{
    public class Spring : GameObject
    {
        public const float Thickness = 2f;

        private readonly Alignment _alignment;
        private readonly Vector2i _startIndex;
        private readonly Vector2i _endIndex;

        public Vector2 StartPoint
        {
            get { return (_startIndex - _alignment.AsUnitVector() * 0.05f + _alignment.Inverse().AsUnitVector() * 0.5f) * Tile.Size; }
        }

        public Vector2 EndPoint
        {
            get { return (_endIndex + _alignment.AsUnitVector() * 1.05f + _alignment.Inverse().AsUnitVector() * 0.5f) * Tile.Size; }
        }

        public override RectangleF Area
        {
            get
            {
                if (_alignment == Alignment.Horizontal)
                {
                    return new RectangleF(this.StartPoint.X, this.StartPoint.Y - Spring.Thickness / 2F, (this.EndPoint.X - this.StartPoint.X), Spring.Thickness);
                }
                else
                {
                    return new RectangleF(this.StartPoint.X - Spring.Thickness / 2f, this.StartPoint.Y, Spring.Thickness, (this.EndPoint.Y - this.StartPoint.Y));
                }
            }
        }

        public override bool CollidesWithPlayer
        {
            get { return false; }
        }

        public bool IsTriggered { get; set; }

        public Spring(Alignment alignment, Vector2i startIndex, Vector2i endIndex)
        {
            _alignment = alignment;
            _startIndex = startIndex;
            _endIndex = endIndex;
        }
    }
}
