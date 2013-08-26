using System;
using System.Collections.Generic;
using System.Linq;
using Flai;
using Flai.Graphics;
using Flai.Ui;
using Microsoft.Xna.Framework;
using WVVW.Editor.Model;
using WVVW.Editor.View;
using WVVW.Model;

namespace WVVW.Editor.Ui
{
    public class TileSelectorList : UiObject
    {
        private readonly EditorPlayer _player;
        public TileSelectorList(EditorPlayer player)
        {
            _player = player;
            this.CreateUi();
        }

        public override void Update(UpdateContext updateContext)
        {
            base.Children.Update(updateContext);
        }

        public override void Draw(GraphicsContext graphicsContext)
        {
            base.Children.Draw(graphicsContext, true);
        }

        private void CreateUi()
        {
            base.Children.Clear();

            const float OffsetX = 0;
            const float OffsetY = Tile.Size * 0.5f;
            const float ButtonSize = 40;
            const int ButtonsInColumn = 14;

            Dictionary<TileType, TileToggleButton> toggleButtons = new Dictionary<TileType, TileToggleButton>();
            TileType[] tileTypes = Enum.GetValues(typeof(TileType)).Cast<TileType>().Where(type => type != TileType.Air).ToArray();
            for (int i = 0; i < tileTypes.Length; i++)
            {
                TileType tileType = tileTypes[i];

                int x = i / ButtonsInColumn;
                int y = i % ButtonsInColumn;
                TileToggleButton toggleButton = new TileToggleButton(new RectangleF(OffsetX + x * ButtonSize, OffsetY + y * ButtonSize, ButtonSize, ButtonSize), tileType);
                toggleButton.Click += (o, e) =>
                {
                    // Can't "de-select" the tower
                    if (_player.CurrentTileType == toggleButton.TileType)
                    {
                        toggleButton.IsToggled = true; // Since ToggleButtonBase automatically un-toggles it, toggle it back
                        return;
                    }

                    if (toggleButtons.ContainsKey(_player.CurrentTileType))
                    {
                        // Un-toggle the previous tile's button
                        toggleButtons[_player.CurrentTileType].IsToggled = false;
                    }

                    // Set the new tile type
                    _player.CurrentTileType = toggleButton.TileType;

                    // And toggle the new toggle button
                    toggleButtons[_player.CurrentTileType].IsToggled = true;
                };

                toggleButtons.Add(tileType, toggleButton);
                base.Children.Add(toggleButton);
            }

            toggleButtons[_player.CurrentTileType].IsToggled = true;
        }

        #region Tile Toggle Button

        private class TileToggleButton : ToggleButtonBase
        {
            private readonly TileType _tileType;
            public TileType TileType
            {
                get { return _tileType; }
            }

            public TileToggleButton(RectangleF area, TileType tileType)
                : base(area, false)
            {
                _tileType = tileType;
            }

            public override void Draw(GraphicsContext graphicsContext)
            {
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, _area, this.IsToggled ? new Color(144, 144, 144) * 0.75f : new Color(64, 64, 64) * 0.55f);

                Vector2 scale = _area.Size / new Vector2(Tile.Size, Tile.Size);
                EditorTileRenderer.DrawTile(graphicsContext, _area.TopLeft, _tileType, this.IsToggled ? Color.White : new Color(192, 192, 192), scale);
            }
        }

        #endregion
    }
}
