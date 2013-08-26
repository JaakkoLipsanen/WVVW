using Flai;
using Flai.Graphics;
using Flai.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WVVW.Editor.Model;
using WVVW.Model;

namespace WVVW.Editor.Controller
{
    public class EditorPlayerController : FlaiController
    {
        private static readonly RectangleF ScreenInputZone = new RectangleF(96, 0, EditorGlobals.EditorScreenWidth - 96, EditorGlobals.EditorScreenHeight - 48);

        private readonly EditorWorld _world;
        private readonly EditorPlayer _player;

        private bool _isResizing = false;
        private Direction2D _resizeDirection;

        public EditorPlayerController(EditorWorld world, EditorPlayer player)
        {
            _world = world;
            _player = player;
        }

        public override void Control(UpdateContext updateContext)
        {
            // Check if the mouse is inside the world
            Vector2 mousePosition = _player.Camera.ScreenToWorld(FlaiGame.Current.GraphicsDevice, updateContext.InputState.MousePosition);
            if (mousePosition.X >= 0 && mousePosition.Y >= 0 &&
                mousePosition.X < _world.Map.Width * Tile.Size && mousePosition.Y < _world.Map.Height * Tile.Size)
            {
                _player.MouseIndex = Tile.WorldToTileCoordinate(mousePosition);
            }
            else
            {
                _player.MouseIndex = null;
            }
            
            this.HandleMovement(updateContext);
            this.HandleZooming(updateContext);

            if (_player.EditorMode == EditorMode.Normal)
            {
                // Lets put this here just to be safe, not sure if necessary
                _isResizing = false;

                this.HandleChangeTile(updateContext);
                this.HandleAddTile(updateContext);
            }
            else if (_player.EditorMode == EditorMode.Resizing)
            {
                this.HandleResizeMap(updateContext);
            }
        }

        #region Handle Movement

        private void HandleMovement(UpdateContext updateContext)
        {
            Vector2 cameraMovement = Vector2.Zero;
            if (updateContext.InputState.IsMouseButtonPressed(MouseButton.Middle))
            {
                cameraMovement -= updateContext.InputState.MousePositionDelta;
            }

            const int ArrowSpeed = 512;
            // Horizontal
            if (updateContext.InputState.IsKeyPressed(Keys.A) || updateContext.InputState.IsKeyPressed(Keys.Left))
            {
                cameraMovement -= Vector2.UnitX * ArrowSpeed * updateContext.DeltaSeconds;
            }
            if (updateContext.InputState.IsKeyPressed(Keys.D) || updateContext.InputState.IsKeyPressed(Keys.Right))
            {
                cameraMovement += Vector2.UnitX * ArrowSpeed * updateContext.DeltaSeconds;
            }

            // Vertical
            if (updateContext.InputState.IsKeyPressed(Keys.W) || updateContext.InputState.IsKeyPressed(Keys.Up))
            {
                cameraMovement -= Vector2.UnitY * ArrowSpeed * updateContext.DeltaSeconds;
            }
            if (updateContext.InputState.IsKeyPressed(Keys.S) || updateContext.InputState.IsKeyPressed(Keys.Down))
            {
                cameraMovement += Vector2.UnitY * ArrowSpeed * updateContext.DeltaSeconds;
            }

            _player.Camera.Position += cameraMovement / _player.Camera.Zoom;
            // Clamp the position
            float ClampOffset = EditorGlobals.EditorScreenHeight / 4f / _player.Camera.Zoom;
            _player.Camera.Position = new Vector2(
                FlaiMath.Clamp(_player.Camera.Position.X, -ClampOffset, _world.Map.Width * Tile.Size + ClampOffset),
                FlaiMath.Clamp(_player.Camera.Position.Y, -ClampOffset, _world.Map.Height * Tile.Size + ClampOffset));
        }

        #endregion

        #region Handle Zooming

        private void HandleZooming(UpdateContext updateContext)
        {
            // Copied from DarkLight

            const float MinimumZoom = 0.01f;
            const float MaximumZoom = 1.5f;

            Camera2D camera = _player.Camera;
            if (updateContext.InputState.ScrollWheelDelta != 0)
            {
                // Calculate the minimum zoom
                float minimumZoom = FlaiMath.Max(MinimumZoom, FlaiMath.Min(0.2f, updateContext.ViewportSize.X / (_world.Map.Width * Tile.Size * 4f), updateContext.ViewportSize.Y / (_world.Map.Height * Tile.Size * 4f)));

                Vector2 mousePosition = camera.ScreenToWorld(updateContext.ViewportSize, updateContext.InputState.MousePosition);
                float previousZoom = camera.Zoom;

                camera.Zoom = MathHelper.Clamp(camera.Zoom + (updateContext.InputState.ScrollWheelDelta / 120f) * 0.175f * camera.Zoom, minimumZoom, MaximumZoom);
                if (updateContext.InputState.ScrollWheelDelta > 0)
                {
                    camera.Position = mousePosition + (previousZoom / camera.Zoom) * (camera.Position - mousePosition);
                }
                else
                {
                    camera.Position = mousePosition + (previousZoom / camera.Zoom) * (camera.Position - mousePosition);
                }
            }
        }

        #endregion

        #region Handle Add/Change Tile

        private void HandleAddTile(UpdateContext updateContext)
        {
            if (_player.MouseIndex.HasValue && EditorPlayerController.ScreenInputZone.Contains(updateContext.InputState.MousePosition))
            {
                if (updateContext.InputState.IsMouseButtonPressed(MouseButton.Left))
                {
                    // Only do the "Add logic" if the tile under the mouse is not the same as the one we are adding
                    if (_world.Map[_player.MouseIndex.Value] != _player.CurrentTileType)
                    {
                        // Don't allow multiple PlayerSpawn's or PlayerGoal's
                        if (!EditorPlayerController.AllowMultipleTiles(_player.CurrentTileType))
                        {
                            // If the current tile type is PlayerSpawn or PlayerGoal, then allow the adding only if the 
                            // click is new
                            if (!updateContext.InputState.IsNewMouseButtonPress(MouseButton.Left))
                            {
                                return;
                            }

                            // Otherwise remove all previous tiles of this type
                            this.ReplaceAllTiles(_player.CurrentTileType, TileType.Air);
                        }

                        // Add the tile to the map
                        _world.Map[_player.MouseIndex.Value] = _player.CurrentTileType;
                        _world.IsModified = true;
                    }
                }
                else if (updateContext.InputState.IsMouseButtonPressed(MouseButton.Right))
                {
                    if (_world.Map[_player.MouseIndex.Value] != TileType.Air)
                    {
                        _world.Map[_player.MouseIndex.Value] = TileType.Air;
                        _world.IsModified = true;
                    }
                }

            }
        }

        // meh this can't use scroll wheel
        private void HandleChangeTile(UpdateContext updateContext)
        {
            return;
            if (updateContext.InputState.ScrollWheelDelta > 0)
            {
                _player.CurrentTileType++;
                if(_player.CurrentTileType > TileType.SpikeLeft)
                {
                    _player.CurrentTileType = (TileType)1; // Skip Air
                }
            }
        }

        private void ReplaceAllTiles(TileType tileType, TileType replacement = TileType.Air)
        {
            for (int y = 0; y < _world.Map.Height; y++)
            {
                for (int x = 0; x < _world.Map.Width; x++)
                {
                    if (_world.Map[x, y] == tileType)
                    {
                        _world.Map[x, y] = replacement;
                    }
                }
            }
        }

        private static bool AllowMultipleTiles(TileType tile)
        {
            return tile != TileType.PlayerSpawn && tile != TileType.PlayerGoal && !Tile.IsStage(tile);
        }

        #endregion

        #region Handle Resize Map

        private void HandleResizeMap(UpdateContext updateContext)
        {
            if (!EditorPlayerController.ScreenInputZone.Contains(updateContext.InputState.MousePosition))
            {
                return;
            }

            if (updateContext.InputState.IsNewMouseButtonPress(MouseButton.Right))
            {
                _isResizing = false;
                _player.EditorMode = EditorMode.Normal;
                return;
            }

            Vector2 mouseWorldCoordinate = _player.Camera.ScreenToWorld(FlaiGame.Current.GraphicsDevice, updateContext.InputState.MousePosition);
            if (_isResizing)
            {
                if (!updateContext.InputState.IsMouseButtonPressed(MouseButton.Left))
                {
                    _isResizing = false;
                    return;
                }

                // To Right and Down, you can resize on per-tile basis
                if (_resizeDirection == Direction2D.Right)
                {
                    int resize = (int)((mouseWorldCoordinate.X - _world.Map.Width * Tile.Size) / Tile.Size / Room.Width) * Room.Width;
                    _world.Map.Resize(Direction2D.Right, resize);
                }
                else if (_resizeDirection == Direction2D.Down)
                {
                    int resize = (int)((mouseWorldCoordinate.Y - _world.Map.Height * Tile.Size) / Tile.Size / Room.Height) * Room.Height;
                    _world.Map.Resize(Direction2D.Down, resize);
                }

                // To Left and Top, you can resize on Room basis
                if (_resizeDirection == Direction2D.Left)
                {
                    int resize = (int)((mouseWorldCoordinate.X / (Room.Width * Tile.Size))) * Room.Width;
                    if (resize != 0)
                    {
                        if (_world.Map.Resize(Direction2D.Left, resize))
                        {
                            _player.Camera.Position -= Vector2.UnitX * resize * Tile.Size;
                        }
                    }
                }
                else if (_resizeDirection == Direction2D.Up)
                {
                    int resize = (int)((mouseWorldCoordinate.Y / (Room.Height * Tile.Size))) * Room.Height;
                    if (resize != 0)
                    {
                        if (_world.Map.Resize(Direction2D.Up, resize))
                        {
                            _player.Camera.Position -= Vector2.UnitY * resize * Tile.Size;
                        }
                    } 
                }
            }
            else
            {
                float MaximumDistance = Tile.Size / _player.Camera.Zoom;
                if (updateContext.InputState.IsMouseButtonPressed(MouseButton.Left))
                {
                    // Left
                    if (FlaiMath.Distance(mouseWorldCoordinate.X, 0) < MaximumDistance)
                    {
                        _isResizing = true;
                        _resizeDirection = Direction2D.Left;
                    }
                    // Right
                    else if (FlaiMath.Distance(mouseWorldCoordinate.X, _world.Map.Width * Tile.Size) < MaximumDistance)
                    {
                        _isResizing = true;
                        _resizeDirection = Direction2D.Right;
                    }
                    // Top
                    else if (FlaiMath.Distance(mouseWorldCoordinate.Y, 0) < MaximumDistance)
                    {
                        _isResizing = true;
                        _resizeDirection = Direction2D.Up;
                    }
                    // Bottom
                    else if (FlaiMath.Distance(mouseWorldCoordinate.Y, _world.Map.Height * Tile.Size) < MaximumDistance)
                    {
                        _isResizing = true;
                        _resizeDirection = Direction2D.Down;
                    }
                }
            }

            _world.IsModified = true;
        }
    }

        #endregion
}
