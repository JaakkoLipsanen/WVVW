using System;
using Flai;
using Microsoft.Xna.Framework;

namespace WVVW.Model
{
    public class TimeBonus : GameObject
    {
        private const float TimeVisibleAfterTaking = 2f;
        private static readonly Vector2 Size = new Vector2(Tile.Size, Tile.Size);

        private readonly Vector2i _index;
        private readonly float _timeBonus;

        private bool _isCollected = false;
        private float _timeSinceTaking = 0f;

        public event GenericEvent<float> OnCollected;

        public Vector2 Position
        {
            get { return _index * Tile.Size; }
        }

        public override RectangleF Area
        {
            get { return new RectangleF(this.Position.X, this.Position.Y, TimeBonus.Size.X, TimeBonus.Size.Y); }
        }

        public override bool CollidesWithPlayer
        {
            get { return false; }
        }

        public bool IsCollected
        {
            get { return _isCollected; }
        }

        public bool IsReadyForRemoving
        {
            get  { return _isCollected && _timeSinceTaking > TimeBonus.TimeVisibleAfterTaking; }
        }

        public TileType TileType
        {
            get { return (_timeBonus == 5f) ? TileType.Clock5 : ((_timeBonus == 10f) ? TileType.Clock10 : TileType.Clock15); }
        }

        public float Time
        {
            get { return _timeBonus; }
        }

        public float Alpha
        {
            get { return (TimeBonus.TimeVisibleAfterTaking - _timeSinceTaking) / TimeBonus.TimeVisibleAfterTaking; } 
        }

        public TimeBonus(Vector2i index, TileType tile)
        {
            if (!Tile.IsClock(tile))
            {
                throw new ArgumentException("Invalid tile!");
            }

            _index = index;
            _timeBonus = (tile == TileType.Clock5) ? 5 : ((tile == TileType.Clock10) ? 10 : 15);
        }

        public TimeBonus(Vector2i index, float timeBonus)
        {
            _index = index;
            _timeBonus = timeBonus;
        }

        public void Update(UpdateContext updateContext)
        {
            if (_isCollected)
            {
                _timeSinceTaking += updateContext.DeltaSeconds;
            }
        }

        public void Collect()
        {
            if (_isCollected)
            {
                return;
            }

            _isCollected = true;
            _timeSinceTaking = 0f;
            this.OnCollected.InvokeIfNotNull(_timeBonus); // Let's hope someone has subscribed to this :P
        }
    }
}
