using Flai;

namespace WVVW.Model
{
    public abstract class GameObject
    {
        public abstract bool CollidesWithPlayer { get; }
        public abstract RectangleF Area { get; }
    }
}
