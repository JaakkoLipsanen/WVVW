using System;
using System.Collections.Generic;
using System.Linq;
using Flai;

namespace WVVW.Model
{
    public enum DoorState
    {
        Locked,
        Unlocking,
        Unlocked
    }

    #region Door Group

    public class DoorGroup
    {
        private const float TimeBetweenUnlocks = 0.1f;

        private readonly Dictionary<Vector2i, Door> _doors;
        private readonly Queue<Door> _nextDoorsToCheck = new Queue<Door>();

        private WorldMap _worldMap;
        private bool _isUnlocking = false;
        private float _timeUntilNextUnlock = 0f;

        public bool IsReadyForRemoval
        {
            get { return _isUnlocking && _doors.Count == 0; }
        }

        public DoorGroup(List<Door> doors)
        {
            _doors = doors.ToDictionary(door => door.WorldIndex);
        }

        public void Update(UpdateContext updateContext)
        {
            if (_isUnlocking)
            {
                _timeUntilNextUnlock -= updateContext.DeltaSeconds;
                if (_timeUntilNextUnlock < 0)
                {
                    _timeUntilNextUnlock += DoorGroup.TimeBetweenUnlocks;
                    this.UnlockOneRound();
                }
            }
        }

        public void SetWorldMap(WorldMap worldMap)
        {
            if (_worldMap != null)
            {
                throw new InvalidOperationException("The world map is already set"); 
            }

            _worldMap = worldMap;
        }

        public void StartUnlocking(Door unlockingStartingDoor)
        {
            if (!_isUnlocking)
            {
                _isUnlocking = true;
                foreach (Door door in _doors.Values)
                {
                    if (door.State != DoorState.Locked)
                    {
                        throw new ArgumentException("Door is already unlocked!");
                    }

                    door.State = DoorState.Unlocking;
                }

                _nextDoorsToCheck.Enqueue(unlockingStartingDoor);
            }
        }

        private void UnlockOneRound()
        {
            int count = _nextDoorsToCheck.Count;
            int i = 0;
            while (i < count)
            {
                this.UnlockDoor(_nextDoorsToCheck.Dequeue());
                i++;
            }
        }

        private void UnlockDoor(Door door)
        {
            if (door.State == DoorState.Unlocking)
            {
                door.State = DoorState.Unlocked;
                _doors.Remove(door.WorldIndex);

                Door value;
                // Left
                if (_doors.TryGetValue(door.WorldIndex - Vector2i.UnitX, out value))
                {
                    if (value.State != DoorState.Unlocked)
                    {
                        _nextDoorsToCheck.Enqueue(_doors[door.WorldIndex - Vector2i.UnitX]);
                    }
                }

                // Right
                if (_doors.TryGetValue(door.WorldIndex + Vector2i.UnitX, out value))
                {
                    if (value.State != DoorState.Unlocked)
                    {
                        _nextDoorsToCheck.Enqueue(_doors[door.WorldIndex + Vector2i.UnitX]);
                    }
                }

                // Up
                if (_doors.TryGetValue(door.WorldIndex - Vector2i.UnitY, out value))
                {
                    if (value.State != DoorState.Unlocked)
                    {
                        _nextDoorsToCheck.Enqueue(_doors[door.WorldIndex - Vector2i.UnitY]);
                    }
                }

                // Down
                if (_doors.TryGetValue(door.WorldIndex + Vector2i.UnitY, out value))
                {
                    if (value.State != DoorState.Unlocked)
                    {
                        _nextDoorsToCheck.Enqueue(_doors[door.WorldIndex + Vector2i.UnitY]);
                    }
                }
            }
        }
    }

    #endregion

    #region Door

    public class Door : GameObject
    {
        private readonly Vector2i _index;
        private DoorGroup _parent;
        private DoorState _doorState = DoorState.Locked;
        private float _alpha = 1f;

        public override RectangleF Area
        {
            get { return new RectangleF(_index.X * Tile.Size, _index.Y * Tile.Size, Tile.Size, Tile.Size); }
        }

        public override bool CollidesWithPlayer
        {
            get { return _doorState != DoorState.Unlocked; }
        }

        public Vector2i WorldIndex
        {
            get { return _index; }
        }

        public DoorState State
        {
            get { return _doorState; }
            set { _doorState = value; }
        }

        public bool IsReadyForRemoval
        {
            get { return _doorState == DoorState.Unlocked && _alpha <= 0; }
        }

        public float Alpha
        {
            get { return _alpha; }
        }

        public DoorGroup Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null)
                {
                    throw new InvalidOperationException("The Parent can't be set after it has already been set once");
                }

                _parent = value;
            }
        }

        public Door(Vector2i index)
        {
            _index = index;
        }

        public void Update(UpdateContext updateContext)
        {
            if(_doorState == DoorState.Unlocked)
            {
                _alpha -= updateContext.DeltaSeconds * 4;
            }
        }

        public void Unlock()
        {
            if (_doorState == DoorState.Locked)
            {
                _parent.StartUnlocking(this);
            }
        }

        public override int GetHashCode()
        {
            return _index.GetHashCode();
        }
    }

    #endregion
}
