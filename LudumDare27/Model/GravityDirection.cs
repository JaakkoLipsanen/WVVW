using Flai;

namespace WVVW.Model
{
    public static class GravityDirectionExtensions
    {
        public static Direction2D AsDirection2D(this GravityDirection gravityDirection)
        {
            return (gravityDirection == GravityDirection.Down) ? Direction2D.Down : Direction2D.Up;
        }

        public static GravityDirection Inverse(this GravityDirection gravityDirection)
        {
            return (gravityDirection == GravityDirection.Down) ? GravityDirection.Up : GravityDirection.Down;
        }
    }

    public enum GravityDirection
    {
        Down = 1,
        Up = -1,
     // Left,
     // Right,
    }
}
