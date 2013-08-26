
using Flai;
using Microsoft.Xna.Framework;
namespace WVVW.Model
{
    public class Player
    {
        public const float Speed = Tile.Size * 18;
        public const float Gravity = Tile.Size * 32;
        private const float JumpTimeBias = 0.1f;
        private static readonly Vector2 Size = new Vector2(Tile.Size * 1.25f, Tile.Size * 2f);

        public event GenericEvent PlayerDied;

        private readonly World _world;
        private Vector2 _position;
        private float _horizontalVelocity = 0f;

        private GravityDirection _gravityDirection = GravityDirection.Down;
        private bool _isAlive = true;

        private float _timeSinceLastJumpRequest = float.MaxValue;
        private float _timeSinceWasOnGround = 0f;

        private Spring _previouslyCollidingSpring = null;
        private int _keys = 0;

        // Center, top-left, bottom-left etc.??
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public float HorizontalVelocity
        {
            get { return _horizontalVelocity; }
            set 
            {
                // Don't check collisions against locks
                if (value != 0 && this.IsTouchingFromDirection(value > 0 ? Direction2D.Right : Direction2D.Left, true, false))
                {
                    value = 0;
                }

                _horizontalVelocity = value; 
            }
        }

        public RectangleF Area
        {
            get { return new RectangleF(_position.X, _position.Y, Player.Size.X, Player.Size.Y); }
        }

        public RectangleF RoundedArea
        {
            get
            {
                RectangleF area = this.Area;
                return new RectangleF(FlaiMath.Round(area.X), FlaiMath.Round(area.Y), area.Width, area.Height);
            }
        }

        public Vector2i RoomIndex
        {
            get { return new Vector2i((_position + Player.Size / 2f) / new Vector2(Room.Width * Tile.Size, Room.Height * Tile.Size)); }
        }

        public bool IsOnGround
        {
            get { return this.IsTouchingFromDirection(_gravityDirection.AsDirection2D()); }
        }

        public bool IsAlive
        {
            get { return _isAlive; }
        }

        public int Keys
        {
            get { return _keys; }
        }

        public Player(World world, Vector2i startingIndex)
        {
            _world = world;
            _position = startingIndex * Tile.Size;
        }

        public void Update(UpdateContext updateContext)
        {
            if (!_isAlive)
            {
                return;
            }

            _position.X += _horizontalVelocity;
            this.HandleCollisions(updateContext, Alignment.Horizontal);
            _horizontalVelocity = 0f;

            // Horizontal movement
            if (!this.IsOnGround)
            {
                _position.Y += Player.Gravity * updateContext.DeltaSeconds * (int)_gravityDirection;
                this.HandleCollisions(updateContext, Alignment.Vertical);
            }

            this.CheckIsTouchingLock(updateContext);
            this.CheckIfNeedsPreOrLateJumping(updateContext);
            this.CheckIfCollectsTimeBonus(updateContext);
            this.CheckIfCollectsKey(updateContext);
            this.CheckIfIsCollidingWithSpring(updateContext);
            this.CheckIfIsCollidingWithBullet(updateContext);
        }

        public void Jump()
        {
            if (_isAlive)
            {
                if (this.IsOnGround)
                {
                    _gravityDirection = _gravityDirection.Inverse();
                }
                else
                {
                    _timeSinceLastJumpRequest = 0f;
                    if (_timeSinceWasOnGround < Player.JumpTimeBias)
                    {
                        _gravityDirection = _gravityDirection.Inverse();
                    }
                }
            }
        }

        public void Kill()
        {
            this.OnDeath();
        }

        private void OnDeath()
        {
            if (_isAlive)
            {
                _isAlive = false;
                this.PlayerDied.InvokeIfNotNull();
            }
        }

        #region Gravity/Collisions
        
        /// Most of the stuff here is from DarkLight

        protected void HandleCollisions(UpdateContext updateContext, Alignment collisionAlignment)
        {
            RectangleF playerBounds = this.RoundedArea;

            // Static Tiles          
            int leftTile = Tile.WorldToTileCoordinate(playerBounds.Left);
            int topTile = Tile.WorldToTileCoordinate(playerBounds.Top);
            int rightTile = Tile.WorldToTileCoordinate(FlaiMath.Ceiling(playerBounds.Right / Tile.Size) * Tile.Size) - 1;
            int bottomTile = Tile.WorldToTileCoordinate(FlaiMath.Ceiling(playerBounds.Bottom / Tile.Size) * Tile.Size) - 1;

            // Tiles
            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    TileType tile = _world.Map[x, y];
                    if (Tile.IsKilling(tile))
                    {
                        this.OnDeath();
                        break;
                    }
                    else if (Tile.IsSolid(tile))
                    {
                        Vector2 depth;
                        if (Player.RectangleIntersectsPlayer(playerBounds, Tile.GetTileBounds(x, y), collisionAlignment, out depth))
                        {
                            if (tile == TileType.DoorLock)
                            {
                                if (_keys > 0)
                                {
                                  //  _world.UnlockDoor(new Vector2i(x, y));
                                    _keys--;
                                    continue;
                                }
                            }

                            _position += depth;
                            playerBounds = this.RoundedArea;
                        }
                    }
                }
            }

            // Game objects
            playerBounds = this.Area;
            foreach (GameObject gameObject in _world.CollisionGameObjects)
            {
                if (gameObject.CollidesWithPlayer)
                {
                    Vector2 depth;
                    if (playerBounds.Intersects(gameObject.Area) && Player.RectangleIntersectsPlayer(playerBounds, gameObject.Area, collisionAlignment, out depth))
                    {
                        if ((this.IsTouchingFromDirection(Direction2D.Down) && depth.Y > 0) ||
                            (this.IsTouchingFromDirection(Direction2D.Left) && depth.X < 0) ||
                            (this.IsTouchingFromDirection(Direction2D.Up) && depth.Y < 0) ||
                            (this.IsTouchingFromDirection(Direction2D.Right) && depth.X > 0))
                        {
                            continue;
                        }

                        _position += depth;
                        playerBounds = this.Area;
                    }
                }
            }
        }

        private bool IsTouchingFromDirection(Direction2D direction, bool checkCollisionAgainstCannons = true, bool checkCollisionsAgainstLocks = true)
        {
            return _world.IsOccupied(this.GetSideAreaPlusOne(direction), checkCollisionAgainstCannons, checkCollisionsAgainstLocks);
        }

        private bool IsTouchingFromDirection(Direction2D direction, out RectangleF collisionArea)
        {
            return _world.IsOccupied(this.GetSideAreaPlusOne(direction), out collisionArea);
        }

        #region Get Side Area

        protected RectangleF GetSideArea(Direction2D side)
        {
            RectangleF fullArea = this.RoundedArea;
            float x = (side != Direction2D.Right) ? fullArea.Left : fullArea.Right - 1;
            float y = (side != Direction2D.Down) ? fullArea.Top : fullArea.Bottom - 1;
            float width = (side == Direction2D.Left || side == Direction2D.Right) ? 1 : fullArea.Width;
            float height = (side == Direction2D.Up || side == Direction2D.Down) ? 1 : fullArea.Height;

            return new RectangleF(x, y, width, height);
        }

        protected RectangleF GetSideAreaPlusOne(Direction2D side)
        {
            RectangleF fullArea = this.RoundedArea;
            if (side == Direction2D.Left)
            {
                return new RectangleF(fullArea.Left - 1, fullArea.Top, 1, fullArea.Height);
            }
            else if (side == Direction2D.Right)
            {
                return new RectangleF(fullArea.Right, fullArea.Top, 1, fullArea.Height);
            }
            else if (side == Direction2D.Up)
            {
                return new RectangleF(fullArea.Left, fullArea.Top - 1, fullArea.Width, 1);
            }
            else // if(side == Direction2D.Down)
            {
                return new RectangleF(fullArea.Left, fullArea.Bottom, fullArea.Width, 1);
            }
        }

        #endregion

        private static bool RectangleIntersectsPlayer(RectangleF playerArea, RectangleF blockArea, Alignment collisionAlignment, out Vector2 depth)
        {
            depth = collisionAlignment == Alignment.Vertical ?
                new Vector2(0, playerArea.GetVerticalIntersectionDepth(blockArea)) :
                new Vector2(playerArea.GetHorizontalIntersectionDepth(blockArea), 0);

            return depth.Y != 0 || depth.X != 0;
        }

        #endregion

        #region CheckXXX  
        
        private void CheckIsTouchingLock(UpdateContext updateContext)
        {
            if (_keys == 0)
            {
                return;
            }

            RectangleF area = this.Area.Inflate(1, 1);
            foreach (Door door in _world.Doors)
            {
                if (area.Intersects(door.Area) && door.State == DoorState.Locked)
                {
                    door.Unlock();
                    _keys--;
                    return;
                }
            }
        }

        private void CheckIfNeedsPreOrLateJumping(UpdateContext updateContext)
        {
            _timeSinceLastJumpRequest += updateContext.DeltaSeconds;
            if (this.IsOnGround)
            {
                _timeSinceWasOnGround = 0f;
                if (_timeSinceLastJumpRequest < Player.JumpTimeBias)
                {
                    _gravityDirection = _gravityDirection.Inverse();
                    _timeSinceLastJumpRequest = float.MaxValue;
                }
            }
            else
            {
                _timeSinceWasOnGround += updateContext.DeltaSeconds;
            }
        }

        private void CheckIfCollectsTimeBonus(UpdateContext updateContext)
        {
            foreach (TimeBonus timeBonus in _world.TimeBonuses)
            {
                if (!timeBonus.IsCollected && this.Area.Intersects(timeBonus.Area))
                {
                    timeBonus.Collect();
                }
            }
        }

        private void CheckIfCollectsKey(UpdateContext updateContext)
        {
            foreach (Key key in _world.Keys)
            {
                if (!key.IsCollected && this.Area.Intersects(key.Area))
                {
                    key.Collect();
                    _keys++;
                }
            }
        }

        private void CheckIfIsCollidingWithSpring(UpdateContext updateContext)
        {
            bool collidedWithAny = false;
            foreach (Spring spring in _world.Springs)
            {
                if (this.Area.Intersects(spring.Area))
                {
                    collidedWithAny = true;
                    if (spring != _previouslyCollidingSpring)
                    {
                        _previouslyCollidingSpring = spring;
                        spring.IsTriggered = true;
                        _gravityDirection = _gravityDirection.Inverse();

                        break;
                    }
                }
            }

            if (!collidedWithAny && _previouslyCollidingSpring != null)
            {
                _previouslyCollidingSpring.IsTriggered = false;
                _previouslyCollidingSpring = null;
            }
        }

        private void CheckIfIsCollidingWithBullet(UpdateContext updateContext)
        {
            RectangleF area = this.Area;
            foreach (Cannon cannon in _world.Cannons)
            {
                foreach (Bullet bullet in cannon.Bullets)
                {
                    if (bullet.IsAlive)
                    {
                        if (area.Intersects(bullet.Area))
                        {
                            this.OnDeath();
                            bullet.Kill();
                            return;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
