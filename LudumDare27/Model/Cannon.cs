using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flai;
using Flai.Extensions;
using Microsoft.Xna.Framework;

namespace WVVW.Model
{
    public class Cannon : GameObject
    {
        private const float TimeBetweenBullets = 2.5f;
        private const float BulletSpeed = Tile.Size * 16f;

        private readonly Direction2D _direction;
        private readonly Vector2i _worldIndex;
        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly ReadOnlyCollection<Bullet> _readOnlyBullets;

        private float _timeUntilNextBullet = 0f;

        public Vector2 CenterPosition
        {
            get { return (_worldIndex + Vector2.One * 0.5f) * Tile.Size; }
        }

        public float Rotation
        {
            get { return _direction.ToRadians(); }
        }

        public ReadOnlyCollection<Bullet> Bullets
        {
            get { return _readOnlyBullets; }
        }

        protected Vector2 BulletSpawnPosition
        {
            get
            {
                return (_worldIndex + Vector2.One * 0.5f) * Tile.Size + _direction.ToUnitVector() * 0.45f; 
            }
        }

        public override RectangleF Area
        {
            get { return new RectangleF(_worldIndex.X * Tile.Size, _worldIndex.Y * Tile.Size, Tile.Size, Tile.Size); }
        }

        public override bool CollidesWithPlayer
        {
            get { return true; }
        }

        public Cannon(TileType tile, Vector2i index)
            : this(Cannon.DirectionFromCannonTile(tile), index)
        {
        }

        public Cannon(Direction2D direction, Vector2i index)
        {
            _direction = direction;
            _worldIndex = index;

            _readOnlyBullets = new ReadOnlyCollection<Bullet>(_bullets);
        }

        public void Update(UpdateContext updateContext)
        {
            _bullets.RemoveAll(bullet => bullet.IsReadyForRemoving);
            foreach (Bullet bullet in _bullets)
            {
                bullet.Update(updateContext);
            }

            // Check if a new bullet should be fired
            _timeUntilNextBullet -= updateContext.DeltaSeconds;
            if (_timeUntilNextBullet < 0)
            {
                _timeUntilNextBullet += Cannon.TimeBetweenBullets;
                this.FireBullet();
            }
        }

        private void FireBullet()
        {
            _bullets.Add(new Bullet(_direction, this.BulletSpawnPosition, Cannon.BulletSpeed));
        }

        private static Direction2D DirectionFromCannonTile(TileType tile)
        {
            switch (tile)
            {
                case TileType.CannonUp: return Direction2D.Up;
                case TileType.CannonRight: return Direction2D.Right;
                case TileType.CannonDown: return Direction2D.Down;
                case TileType.CannonLeft: return Direction2D.Left;

                 default:
                    throw new ArgumentException("Tile is not a cannon!");
            }
        }
    }
}
