using Flai;
using Flai.Extensions;
using Microsoft.Xna.Framework;

namespace WVVW.Model
{
    // Okay this is super-not-efficient since this is a class. But.. Could this be a struct? hmm..
    // Or maybe do some pooling
    public class Bullet
    {
        public static readonly Vector2 Size = new Vector2(Tile.Size / 2f, Tile.Size);

        private readonly Direction2D _direction;
        private readonly float _speed;

        private Vector2 _position;
        private bool _isAlive = true;
        private float _alpha = 0f;

        public Vector2 Position
        {
            get { return _position; }
        }

        public RectangleF Area
        {
            get
            {
                // Vertical
                if (_direction == Direction2D.Up || _direction == Direction2D.Down)
                {
                    return new RectangleF(_position.X - Bullet.Size.X / 2f, _position.Y - Bullet.Size.Y / 2f, Bullet.Size.X, Bullet.Size.Y); 
                }
                // Horizontal
                else
                {
                    return new RectangleF(_position.X - Bullet.Size.Y / 2f, _position.Y - Bullet.Size.X / 2f, Bullet.Size.Y, Bullet.Size.X);
                }
            }
        }

        public float Rotation
        {
            get { return _direction.ToRadians(); }
        }

        public bool IsAlive
        {
            get { return _isAlive; }
        }

        public bool IsReadyForRemoving
        {
            get { return !_isAlive && _alpha <= 0; }
        }

        public float Alpha
        {
            get { return _alpha; }
        }

        public Bullet(Direction2D direction, Vector2 initialPosition, float speed)
        {
            _direction = direction;
            _position = initialPosition;
            _speed = speed;
        }

        public void Update(UpdateContext updateContext)
        {
            if (_isAlive)
            {
                _alpha = FlaiMath.Min(_alpha + updateContext.DeltaSeconds * 4f, 1);
                _position += _direction.ToUnitVector() * _speed * updateContext.DeltaSeconds;
            }
            else
            {
                _alpha -= updateContext.DeltaSeconds * 2;
            }
        }

        public void Kill()
        {
            if (_isAlive)
            {
                _isAlive = false;
            }
        }
    }
}