using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flai;
using Flai.DataStructures;

namespace WVVW.Model
{
    public class WorldMap
    {
        private readonly Vector2i[] _stageIndexes;
        private readonly ReadOnlyArray<Vector2i> _readOnlyStageIndexes;
        private readonly TileType[] _tiles;
        private readonly int _width;
        private readonly int _height;
        private readonly int _hash;

        private readonly Vector2i _startingIndex;
        private readonly Vector2i _goalIndex;

        public int Hash
        {
            get { return _hash; }
        }

        public int StageCount
        {
            get { return _stageIndexes.Length + 1; } // + 1???? 
        }

        public ReadOnlyArray<Vector2i> StageIndexes
        {
            get { return _readOnlyStageIndexes; }
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public int WidthInRooms
        {
            get { return _width / Room.Width; }
        }

        public int HeightInRooms
        {
            get { return _height / Room.Height; }
        }

        public Vector2i StartingIndex
        {
            get { return _startingIndex; }
        }

        public Vector2i GoalIndex
        {
            get { return _goalIndex; }
        }

        private WorldMap(TileType[] tiles, int width, int height, Vector2i startingIndex, Vector2i goalIndex, Vector2i[] stageIndexes, int hash)
        {
            if (tiles.Length != width * height)
            {
                throw new ArgumentException("");
            }

            _stageIndexes = stageIndexes;
            _tiles = tiles;
            _width = width;
            _height = height;
            _hash = hash;

            _startingIndex = startingIndex;
            _goalIndex = goalIndex;

            _readOnlyStageIndexes = new ReadOnlyArray<Vector2i>(_stageIndexes);
        }

        public TileType this[int x, int y]
        {
            get 
            {
                if (x < 0 || y < 0 || x >= _width || y >= _height)
                {
                    return TileType.Solid;
                }

                return _tiles[x + y * _width]; 
            }
        }

        public TileType this[Vector2i index]
        {
            get 
            {
                if (index.X < 0 || index.Y < 0 || index.X >= _width || index.Y >= _height)
                {
                    return TileType.Solid;
                }

                return _tiles[index.X + index.Y * _width];
            }
        }

        public static WorldMap Load(BinaryReader reader, out List<GameObject> gameObjects, out List<DoorGroup> doorGroups)
        {
            int hash = reader.ReadInt32();
            int stageCount = reader.ReadInt32(); // not used, calculated by finding all stage tiles since the index is needed too
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            Vector2i? startingIndex = null;
            Vector2i? goalIndex = null;

            gameObjects = new List<GameObject>();
            doorGroups = new List<DoorGroup>();

            List<Tuple<int, Vector2i>> stages = new List<Tuple<int, Vector2i>>();
            TileType[] tiles = new TileType[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileType tile = (TileType)reader.ReadByte();
                    if(tile == TileType.PlayerSpawn)
                    {
                        startingIndex = new Vector2i(x, y);
                        tile = TileType.Air;
                    }
                    else if(tile == TileType.PlayerGoal)
                    {
                        goalIndex = new Vector2i(x, y);
                        tile = TileType.Air;
                    }
                    else if (Tile.IsClock(tile))
                    {
                        gameObjects.Add(new TimeBonus(new Vector2i(x, y), tile));
                        tile = TileType.Air;
                    }
                    else if (tile == TileType.Key)
                    {
                        gameObjects.Add(new Key(new Vector2i(x, y)));
                        tile = TileType.Air;
                    }
                    else if (Tile.IsCannon(tile))
                    {
                        gameObjects.Add(new Cannon(tile, new Vector2i(x, y)));

                        // Meh.. let's keep the tile as Cannon so that collisions work without a hassle
                      //  tile = TileType.Air;
                    }
                    else if (Tile.IsStage(tile))
                    {
                        stages.Add(new Tuple<int, Vector2i>((int)tile - (int)TileType.Stage1 + 1, new Vector2i(x, y)));
                        tile = TileType.Air;
                    }

                    tiles[x + y * width] = tile;
                }
            }

            // Post-process tiles (remove springs etc)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileType tile = tiles[x + y * width];
                    if (Tile.IsSpring(tile))
                    {
                        gameObjects.Add(WorldMap.RemoveSpring(tiles, x, y, width, height));
                    }
                    else if (tile == TileType.DoorLock)
                    {
                        doorGroups.Add(WorldMap.RemoveDoor(tiles, gameObjects, x, y, width, height));
                    }
                }
            }

            if (!startingIndex.HasValue || !goalIndex.HasValue)
            {
                throw new ArgumentException("No starting point or goal found!");
            }

            return new WorldMap(tiles, width, height, startingIndex.Value, goalIndex.Value, stages.OrderBy(t => t.Item1).Select(t => t.Item2).ToArray(), hash);
        }

        private static Spring RemoveSpring(TileType[] tiles, int x, int y, int width, int height)
        {
            TileType targetSpringType = tiles[x + y * width];
            if (!Tile.IsSpring(targetSpringType))
            {
                throw new ArgumentException("The tile is not a spring");
            }
            
            Vector2i step = (targetSpringType == TileType.SpringHorizontal) ? Vector2i.UnitX : Vector2i.UnitY;
            Vector2i startIndex = new Vector2i(x, y) - step;
            Vector2i endIndex = new Vector2i(x, y) + step;

            // Set the starting tile to Air
            tiles[x + y * width] = TileType.Air;

            // Move start index
            while ((startIndex.X >= 0 && startIndex.Y >= 0 && startIndex.X < width && startIndex.Y < height) &&
                tiles[startIndex.X + startIndex.Y * width] == targetSpringType)
            {
                tiles[startIndex.X + startIndex.Y * width] = TileType.Air;
                startIndex -= step;
            }
            startIndex += step;

            // Move end index
            while ((endIndex.X >= 0 && endIndex.Y >= 0 && endIndex.X < width && endIndex.Y < height) &&
                tiles[endIndex.X + endIndex.Y * width] == targetSpringType)
            {
                tiles[endIndex.X + endIndex.Y * width] = TileType.Air;
                endIndex += step;
            }
            endIndex -= step;

            return new Spring((targetSpringType == TileType.SpringHorizontal) ? Alignment.Horizontal : Alignment.Vertical, startIndex, endIndex);

        }

        private static DoorGroup RemoveDoor(TileType[] tiles, List<GameObject> gameObjects, int x, int y, int width, int height)
        {
            if (tiles[x + y * width] != TileType.DoorLock)
            {
                throw new ArgumentException("The tile is not a door");
            }

            List<Door> doors = new List<Door>();
            WorldMap.ProcessRemoveDoor(tiles, doors, x, y, width, height);

            DoorGroup doorGroup = new DoorGroup(doors);
            foreach (Door door in doors)
            {
                door.Parent = doorGroup;
                gameObjects.Add(door);
            }

            return doorGroup;
        }

        private static void ProcessRemoveDoor(TileType[] tiles,List<Door> doors, int x, int y, int width, int height)
        {
            if(x >= 0 && y >= 0 && x < width && y < height && tiles[x + y * width] == TileType.DoorLock)
            {
                tiles[x + y * width] = TileType.Air;
                doors.Add(new Door(new Vector2i(x, y)));

                WorldMap.ProcessRemoveDoor(tiles, doors, x - 1, y, width, height);
                WorldMap.ProcessRemoveDoor(tiles, doors, x + 1, y, width, height);
                WorldMap.ProcessRemoveDoor(tiles, doors, x, y - 1, width, height);
                WorldMap.ProcessRemoveDoor(tiles, doors, x, y + 1, width, height);
            }
        }

        public Vector2i GetStageIndex(int stage)
        {
            return  _stageIndexes[stage - 1];
        }
    }
}
