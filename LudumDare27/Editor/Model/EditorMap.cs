using System;
using System.Collections.Generic;
using System.IO;
using Flai;
using Flai.Extensions;
using Microsoft.Xna.Framework;
using WVVW.Model;

namespace WVVW.Editor.Model
{
    public class EditorMap
    {
        private const int MinimumWidth = Room.Width;
        private const int MinimumHeight = Room.Height;

        private TileType[] _tiles;
        private int _width;
        private int _height;

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

        public EditorMap(TileType[] tiles, int width, int height)
        {
            if (tiles.Length != width * height)
            {
                throw new ArgumentException("");
            }

            _tiles = tiles;
            _width = width;
            _height = height;
        }

        public TileType this[int x, int y]
        {
            get { return _tiles[x + y * _width]; }
            set { _tiles[x + y * _width] = value; }
        }

        public TileType this[Vector2i index]
        {
            get { return _tiles[index.X + index.Y * _width]; }
            set { _tiles[index.X + index.Y * _width] = value; }
        }

        public static EditorMap Generate()
        {
            return new EditorMap(new TileType[Room.Width * Room.Height], Room.Width, Room.Height);
        }

        #region Resize

        public bool Resize(Direction2D resizeDirection, int difference)
        {
            if(difference == 0)
            {
                return false;
            }

            if (resizeDirection == Direction2D.Right)
            {
                int newWidth = FlaiMath.Max(EditorMap.MinimumWidth, _width + difference);
                if (newWidth == _width)
                {
                    return false;
                }

                this.Resize(_width + difference, _height);
            }
            else if (resizeDirection == Direction2D.Down)
            {
                int newHeight = FlaiMath.Max(EditorMap.MinimumHeight, _height + difference);
                if (newHeight == _height)
                {
                    return false;
                }

                this.Resize(_width, _height + difference);
            }

            if (resizeDirection == Direction2D.Left)
            {
                int newWidth = FlaiMath.Max(EditorMap.MinimumWidth, _width - difference);
                if (newWidth == _width)
                {
                    return false;
                }

                // Make the world smaller
                if (difference > 0)
                {
                    this.MoveTiles(new Rectangle(difference, 0, _width - difference, _height), new Vector2i(-difference, 0));
                    this.Resize(_width - difference, _height);
                }
                // Make the world bigger
                else
                {
                    this.Resize(_width - difference, _height);
                    this.MoveTiles(new Rectangle(0, 0, _width + difference, _height), new Vector2i(-difference, 0));
                }
            }
            else if (resizeDirection == Direction2D.Up)
            {
                int newHeight = FlaiMath.Max(EditorMap.MinimumHeight, _height - difference);
                if (newHeight == _height)
                {
                    return false;
                }

                // Make the world smaller
                if (difference > 0)
                {
                    this.MoveTiles(new Rectangle(0, difference, _width, _height - difference), new Vector2i(0, -difference));
                    this.Resize(_width, _height - difference);
                }
                // Make the world bigger
                else
                {
                    this.Resize(_width, _height - difference);
                    this.MoveTiles(new Rectangle(0, 0, _width, _height + difference), new Vector2i(0, -difference));
                }
            }

            return true;
        }

        private void Resize(int newWidth, int newHeight)
        {
            newWidth = FlaiMath.Max(EditorMap.MinimumWidth, newWidth);
            newHeight = FlaiMath.Max(EditorMap.MinimumHeight, newHeight);
            if (newWidth == _width && newHeight == _height)
            {
                return;
            }

            TileType[] newTiles = new TileType[newWidth * newHeight];
            newTiles.Populate(TileType.Air); // not very fast. could be done faster

            int lastY = Math.Min(_height, newHeight);
            int lastX = Math.Min(_width, newWidth);
            for (int y = 0; y < lastY; y++)
            {
                for (int x = 0; x < lastX; x++)
                {
                    newTiles[x + y * newWidth] = _tiles[x + y * _width];
                }
            }

            _width = newWidth;
            _height = newHeight;
            _tiles = newTiles;
        }

        #endregion

        #region Move Tiles

        private void MoveTiles(Rectangle tileArea, Vector2i offset)
        {
            Rectangle newArea = new Rectangle(tileArea.X + offset.X, tileArea.Y + offset.Y, tileArea.Width, tileArea.Height);

            int startX;
            int endX;
            int xDir;

            if (offset.X > 0)
            {
                startX = Math.Min(this.Width - 1, newArea.Right);
                endX = Math.Max(0, newArea.Left);
                xDir = -1;
            }
            else
            {
                startX = Math.Max(0, newArea.Left);
                endX = Math.Min(this.Width - 1, newArea.Right);
                xDir = 1;
            }

            int startY;
            int endY;
            int yDir;

            if (offset.Y > 0)
            {
                startY = Math.Min(this.Height - 1, newArea.Bottom);
                endY = Math.Max(0, newArea.Top);
                yDir = -1;
            }
            else
            {
                startY = Math.Max(0, newArea.Top);
                endY = Math.Min(this.Height - 1, newArea.Bottom);
                yDir = 1;
            }

            if(!((xDir >= 0 == startX <= endX) && (yDir >= 0 == startY <= endY)))
            {
                return;
            }

            for (int y = startY; y != (endY + yDir); y += yDir)
            {
                for (int x = startX; x != (endX + xDir); x += xDir)
                {
                    Vector2i sourceIndex = new Vector2i(x, y) - offset;
                    bool isOutOfBounds = sourceIndex.X < 0 || sourceIndex.Y < 0 || sourceIndex.X >= _width || sourceIndex.Y >= _height;

                    this[x, y] = isOutOfBounds ? TileType.Air : this[sourceIndex];
                    if (!isOutOfBounds && !newArea.Contains(sourceIndex))
                    {
                        this[sourceIndex] = TileType.Air;
                    }
                }
            }
        }

        #endregion

        public void Save(BinaryWriter writer)
        {
            writer.Write(this.CalculateHash()); // hash
            writer.Write(this.CalculateStageCount()); 
            writer.Write(_width);
            writer.Write(_height);

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    writer.Write((byte)this[x, y]);
                }
            }
        }

        public bool IsPlayable(out string error)
        {
            // Check if contains start and goal
            bool containsStart = false;
            bool containsGoal = false;
            List<int> stages = new List<int>();
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (this[x, y] == TileType.PlayerSpawn)
                    {
                        containsStart = true;
                    }
                    else if (this[x, y] == TileType.PlayerGoal)
                    {
                        containsGoal = true;
                    }
                    else if (Tile.IsStage(this[x, y]))
                    {
                        stages.Add((int)this[x, y] - (int)TileType.Stage1 + 1);
                    }
                }
            }

            if (!containsStart)
            {
                error = "The level does not contain a starting point (PlayerSpawn)";
                return false;
            }

            if (!containsGoal)
            {
                error = "The level does not contain a goal (PlayerGoal)";
                return false;
            }

            // Check if contains sequental stages
            stages.Sort();
            for (int i = 0; i < stages.Count; i++)
            {
                if (stages[i] != i + 1)
                {
                    error = "The Stages are not consecutive should be { (1, 2, 3, 4, 5) (1, 2, 3) (1, 2, 3, 4, 5, 6, 7, 8) } etc not { (1, 3, 4, 5) or (2, 3, 4, 5, 8) }!";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private int CalculateHash()
        {
            int hash = 0;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    // please work :(
                    hash += (hash % 5) + ((int)this[x, y] + hash) % 23 + (int)this[x, y]; // let it overflow if it wants to :P !
                }
            }

            return hash;
        }

        private int CalculateStageCount()
        {
            int stageCount = 1; // start -> end is 1 "stage"
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (Tile.IsStage(this[x, y]))
                    {
                        stageCount++;
                    }
                }
            }

            return stageCount;
        }

        public static EditorMap Load(BinaryReader reader)
        {
            int hash = reader.ReadInt32(); // YEAAAH!! not gonna do anything with this though probably :P
            int stageCount = reader.ReadInt32(); // BOOYAAH!! not gonna use this either
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            EditorMap editorMap = new EditorMap(new TileType[width * height], width, height);
            for (int y = 0; y < editorMap.Height; y++)
            {
                for (int x = 0; x < editorMap.Width; x++)
                {
                    editorMap[x, y] = (TileType)reader.ReadByte();
                }
            }

            return editorMap;
        }
    }
}
