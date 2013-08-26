using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using WVVW.Model;

namespace WVVW.Editor.Model
{
    public enum EditorMode
    {
        Normal,
        Resizing,
    }

    public class EditorPlayer
    {
        // Since the actual... "model"/controlling depends on the Camera's position/zoom/rotation etc,
        // let's just put the camera here instead of in the View
        private readonly Camera2D _camera = new Camera2D(new Vector2(Room.Width * Tile.Size / 2f, Room.Height * Tile.Size / 2f)) { Zoom = 0.75f };
        private Vector2i? _mouseIndex = null;
        private TileType _currentTileType = TileType.Solid;
        private EditorMode _editorMode = EditorMode.Normal;

        public Vector2i? MouseIndex
        {
            get { return _mouseIndex; }
            set { _mouseIndex = value; } // ..? Needs setter atm because it is set from PlayerController
        }

        public TileType CurrentTileType
        {
            get { return _currentTileType; }
            set 
            { 
                _currentTileType = value;
                if (_editorMode != EditorMode.Normal)
                {
                    this.EditorMode = EditorMode.Normal;
                }
            }
        }

        public EditorMode EditorMode
        {
            get { return _editorMode; }
            set { _editorMode = value; }
        }

        public Camera2D Camera
        {
            get { return _camera; }
        }

        public EditorPlayer()
        {
        }
    }
}
