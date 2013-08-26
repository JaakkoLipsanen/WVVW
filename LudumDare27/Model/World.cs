using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Flai;

namespace WVVW.Model
{
    public class World
    {
        private readonly WorldMap _worldMap;
        private Portal _stagePortal; // Can be null

        private readonly List<TimeBonus> _timeBonuses;
        private readonly ReadOnlyCollection<TimeBonus> _readOnlyTimeBonuses;

        private readonly List<Spring> _springs;
        private readonly ReadOnlyCollection<Spring> _readOnlySprings;

        private readonly List<Key> _keys;
        private readonly ReadOnlyCollection<Key> _readOnlyKeys;

        private readonly List<Cannon> _cannons;
        private readonly ReadOnlyCollection<Cannon> _readOnlyCannons;

        private readonly List<Door> _doors;
        private readonly ReadOnlyCollection<Door> _readOnlyDoors;

        private readonly List<DoorGroup> _doorGroups = new List<DoorGroup>();

        public Vector2i PlayerSpawnIndex
        {
            get { return _worldMap.StartingIndex; }
        }

        public Vector2i PlayerGoalIndex
        {
            get { return _worldMap.GoalIndex; }
        }

        public WorldMap Map
        {
            get { return _worldMap; }
        }

        public ReadOnlyCollection<TimeBonus> TimeBonuses
        {
            get { return _readOnlyTimeBonuses; }
        }

        public ReadOnlyCollection<Spring> Springs
        {
            get { return _readOnlySprings; }
        }

        public ReadOnlyCollection<Key> Keys
        {
            get { return _readOnlyKeys; }
        }

        public ReadOnlyCollection<Cannon> Cannons
        {
            get { return _readOnlyCannons; }
        }

        public ReadOnlyCollection<Door> Doors
        {
            get { return _readOnlyDoors; }
        }

        public Portal Portal
        {
            get { return _stagePortal; }
        }

        public IEnumerable<GameObject> CollisionGameObjects
        {
            get
            {
                // MEH!!! this is awful! I think the whole process to make all inherit from "GameObject"
                // was awful.. Fuck!
                foreach (Cannon cannon in _cannons)
                {
                    yield return cannon;
                }

                foreach (Door door in _doors)
                {
                    yield return door;
                }
            }
        }

        protected World(WorldMap worldMap, List<GameObject> gameObjects, List<DoorGroup> doorGroups)
        {
            _worldMap = worldMap;
            _timeBonuses = gameObjects.OfType<TimeBonus>().ToList();
            _springs = gameObjects.OfType<Spring>().ToList();
            _keys = gameObjects.OfType<Key>().ToList();
            _cannons = gameObjects.OfType<Cannon>().ToList();
            _doors = gameObjects.OfType<Door>().ToList();
            _doorGroups = doorGroups;

            _readOnlyTimeBonuses = new ReadOnlyCollection<TimeBonus>(_timeBonuses);
            _readOnlySprings = new ReadOnlyCollection<Spring>(_springs);
            _readOnlyKeys = new ReadOnlyCollection<Key>(_keys);
            _readOnlyCannons = new ReadOnlyCollection<Cannon>(_cannons);
            _readOnlyDoors = new ReadOnlyCollection<Door>(_doors);
        }

        public void Update(UpdateContext updateContext)
        {
            foreach (TimeBonus timeBonus in _timeBonuses)
            {
                timeBonus.Update(updateContext);
            }
            _timeBonuses.RemoveAll(timeBonus => timeBonus.IsReadyForRemoving);

            foreach (Door door in _doors)
            {
                door.Update(updateContext);
            }
            _doors.RemoveAll(door => door.IsReadyForRemoval);

          /*  foreach (UnlockDoorAction unlockAction in _unlockDoorActions)
            {
                unlockAction.Update(updateContext);
            }
            _unlockDoorActions.RemoveAll(unlockAction => unlockAction.IsCompleted); */

            foreach (DoorGroup doorGroup in _doorGroups)
            {
                doorGroup.Update(updateContext);
            }
            _doorGroups.RemoveAll(group => group.IsReadyForRemoval);

            foreach (Cannon cannon in _cannons)
            {
                cannon.Update(updateContext);
            }
            this.CheckBulletCollisions(updateContext);
        }

      /*  public bool UnlockDoor(Vector2i index)
        {
            TileType tile = _worldMap[index];
            if (tile == TileType.DoorLock)
            {
                // meh! this sucks
                foreach (UnlockDoorAction unlockAction in _unlockDoorActions)
                {
                    if (unlockAction.IsUnlocking(index))
                    {
                        return false;
                    }
                }

                _unlockDoorActions.Add(new UnlockDoorAction(_worldMap, index));
                return true;
            }

            return false;
        } */

        // MEH! this "checkCollisionAgainstCannons" thing sucks
        public bool IsOccupied(RectangleF area, bool checkCollisionAgainstCannons = true, bool checkCollisionsAgaisnstLocks = true)
        {
            RectangleF collisionArea;
            return this.IsOccupied(area, out collisionArea, checkCollisionAgainstCannons, checkCollisionsAgaisnstLocks);
        }

        public bool IsOccupied(RectangleF area, out RectangleF collisionArea, bool checkCollisionAgainstCannons = true, bool checkCollisionsAgainstDoors = true)
        {
            /* Tiles */
            int leftTile = Tile.WorldToTileCoordinate(area.Left);
            int topTile = Tile.WorldToTileCoordinate(area.Top);
            int rightTile = Tile.WorldToTileCoordinate(FlaiMath.Ceiling(area.Right / Tile.Size) * Tile.Size) - 1;
            int bottomTile = Tile.WorldToTileCoordinate(FlaiMath.Ceiling(area.Bottom / Tile.Size) * Tile.Size) - 1;

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    if (Tile.IsSolid(_worldMap[x, y]) && (checkCollisionAgainstCannons || !Tile.IsCannon(_worldMap[x, y])) && (checkCollisionsAgainstDoors || _worldMap[x, y] != TileType.DoorLock))
                    {
                        collisionArea = Tile.GetTileBounds(x, y);
                        return true;
                    }
                }
            }

            // Game objects
            foreach (GameObject gameObject in this.CollisionGameObjects)
            {
                if (gameObject.CollidesWithPlayer)
                {
                    if (area.Intersects(gameObject.Area) && area.Intersects(gameObject.Area))
                    {
                        if (!checkCollisionAgainstCannons && gameObject.GetType() == typeof(Cannon))
                        {
                            continue;
                        }

                        if (!checkCollisionsAgainstDoors && gameObject.GetType() == typeof(Door))
                        {
                            continue;
                        }

                        collisionArea = gameObject.Area; // !!!! ......
                        return true;
                    }
                }
            }

            collisionArea = default(RectangleF);
            return false;
        }

        public Vector2i GetStageStartIndex(int stage)
        {
            return (stage == 0) ? this.PlayerSpawnIndex : ((stage >= _worldMap.StageCount) ? this.PlayerGoalIndex : _worldMap.GetStageIndex(stage));
        }

        // Meh!!! But this must be done here unless I give the world map as a parameter
        // to Cannon and Bullet.. Idk..
        private void CheckBulletCollisions(UpdateContext updateContext)
        {
            foreach (Cannon cannon in _cannons)
            {
                foreach (Bullet bullet in cannon.Bullets)
                {
                    if (this.IsOccupied(bullet.Area, false))
                    {
                        bullet.Kill();
                    }
                }
            }
        }

        public static World Load(BinaryReader reader)
        {
            List<GameObject> gameObjects;
            List<DoorGroup> doorGroups;

            WorldMap map = WorldMap.Load(reader, out gameObjects, out doorGroups);
            doorGroups.ForEach(group => group.SetWorldMap(map));

            return new World(map, gameObjects, doorGroups);
        }

        public Portal CreatePortal(Vector2i index)
        {
            _stagePortal = new Portal(index);
            return _stagePortal;
        }

        public void ReplaceStagesWithTimeBonuses()
        {
            foreach (Vector2i index in _worldMap.StageIndexes)
            {
                _timeBonuses.Add(new TimeBonus(index, TileType.Clock10));
            }
        }
    }
}
