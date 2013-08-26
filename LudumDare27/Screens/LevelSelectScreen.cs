using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Flai;
using Flai.Graphics;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

namespace WVVW.Screens
{
    public class LevelSelectScreen : GameScreen
    {
        private readonly BasicUiContainer _uiContainer = new BasicUiContainer();
        private readonly string[] _files;
        private readonly ScoreFile _scoreFile;
        private readonly string _initialLevel;

        private bool _reloadLevel = false;
        private int _currentLevelIndex = 0;
        private LevelScore _currentLevelScore;

        public LevelSelectScreen(string initialLevel)
        {
            _initialLevel = initialLevel;
            _files = LevelSelectScreen.FindAllLevelFiles();
            if (_files != null && _files.Length > 0)
            {
                _scoreFile = ScoreFile.Open();
            }
        }

        protected override void LoadContent()
        {
            if (_files == null || _files.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("No levels found!");
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            }
            else
            {
                int index = -1;
                if (_initialLevel != null)
                {
                    index = Array.FindIndex(_files, filePath => filePath == _initialLevel);
                }

                if (index < 0)
                {
                    index = Array.FindIndex(_files, filePath => filePath.EndsWith("Out of Oxygen.wvvw"));
                    if (index >= 0)
                    {
                        _currentLevelIndex = index;
                    }
                }

                if (index < 0)
                {
                    index = 0;
                }

                _currentLevelIndex = index;
                this.LoadLevelData();
            }
        }

        protected override void UnloadContent()
        {
            if (_scoreFile != null)
            {
                _scoreFile.Save();
            }
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (this.IsActive)
            {
                if (updateContext.InputState.IsNewKeyPress(XnaKeys.Escape))
                {
                    LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
                    return;
                }

                _uiContainer.Update(updateContext);
                if (_reloadLevel)
                {
                    this.LoadLevelData();
                }
            }
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp);

            // Draw level name
            const string LevelNameFont = "Minecraftia40";
            graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[LevelNameFont], Path.GetFileNameWithoutExtension(_files[_currentLevelIndex]), new Vector2(graphicsContext.ScreenSize.X / 2f, 96), Color.White);

            // Draw other ui
            _uiContainer.Draw(graphicsContext, true);

            graphicsContext.SpriteBatch.End();

        }

        private void LoadLevelData()
        {
            if (_currentLevelIndex < _files.Length)
            {
                int hash;
                int stageCount;
                using (var stream = File.OpenRead(_files[_currentLevelIndex]))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        hash = reader.ReadInt32();
                        stageCount = reader.ReadInt32();
                    }
                }

                if (_scoreFile.ContainsHash(hash))
                {
                    _currentLevelScore = _scoreFile.GetLevelScore(hash);
                }
                else
                {
                    _currentLevelScore = new LevelScore(hash, stageCount);
                    _scoreFile.AddLevelScore(_currentLevelScore);
                }

                this.CreateUi();
            }
        }

        private void CreateUi()
        {
            _uiContainer.Clear();
            Vector2 screenSize = base.Game.WindowSize;

            // Full map playthrough button
            LevelSelectButton fullMapButton = new LevelSelectButton("Full map", _currentLevelScore.MapBestTime, new RectangleF(screenSize.X / 2f - 96, 144, 96 * 2, 48));
            fullMapButton.Click += (o, e) =>
            {
                LoadingScreen.Load(base.ScreenManager, false, new GameplayScreen(_files[_currentLevelIndex], GameplayMode.FullRun));
            };
            _uiContainer.Add(fullMapButton);

            if (_currentLevelScore.StageCount > 1)
            {
                for (int i = 0; i < _currentLevelScore.StageCount; i++)
                {
                    int x = i % 4;
                    int y = i / 4;

                    int stageIndex = i + 1;
                    LevelSelectButton stageButton = new LevelSelectButton("Stage " + stageIndex, _currentLevelScore.StageBestTimes[i], new RectangleF(80 + screenSize.X / 4 * x - 64, screenSize.Y / 2 + 64 + y * 112 - 24, 64 * 2, 24 * 2));
                    stageButton.Click += (o, e) =>
                    {
                        LoadingScreen.Load(base.ScreenManager, false, new GameplayScreen(_files[_currentLevelIndex], stageIndex - 1));
                    };
                    _uiContainer.Add(stageButton);
                }
            }

            // Arrows
            const int Scale = 4;
            Texture2D arrowTexture = base.ContentProvider.DefaultManager.LoadTexture("Arrow");
            Vector2 Size = new Vector2(arrowTexture.Width, arrowTexture.Height) * Scale;

            // Previous
            TexturedButton previousButton = new TexturedButton(new RectangleF(32, 32, Size.X, Size.Y), new Sprite(arrowTexture, false) {Scale = new Vector2(Scale), SpriteEffects = SpriteEffects.FlipHorizontally });
            previousButton.Enabled = _files.Length > 1;
            previousButton.Sprite.Tint = previousButton.Enabled ? Color.White : Color.Gray;
            previousButton.Click += (o, e) =>
            {
                _currentLevelIndex--;
                if (_currentLevelIndex < 0)
                {
                    _currentLevelIndex = _files.Length - 1;
                }

                _reloadLevel = true;
            };
            _uiContainer.Add(previousButton);

            // Next
            TexturedButton nextButton = new TexturedButton(new RectangleF(screenSize.X - Size.X - 32, 32, Size.X, Size.Y), new Sprite(arrowTexture, false) { Scale = new Vector2(Scale) });
            nextButton.Enabled = _files.Length > 1;
            nextButton.Sprite.Tint = previousButton.Enabled ? Color.White : Color.Gray;
            nextButton.Click += (o, e) =>
            {
                _currentLevelIndex++;
                if (_currentLevelIndex >= _files.Length)
                {
                    _currentLevelIndex = 0;
                }

                _reloadLevel = true;
            };
            _uiContainer.Add(nextButton);
        }

        private static string[] FindAllLevelFiles()
        {
            string path = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "Levels");
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                // That Where is probably unnecessary but whatever
                return directoryInfo.GetFiles("*" + WvvwGlobals.FileExtension).Where(fileInfo => fileInfo.Exists).Select(fileInfo => fileInfo.FullName).ToArray();
            }

            return null;
        }

        private class LevelSelectButton : ButtonBase
        {
            private readonly string _text;
            private readonly string _scoreText;

            public LevelSelectButton(string text, float? time, RectangleF area)
                : base(area)
            {
                _text = text;

                if (time.HasValue)
                {
                    _scoreText = time.Value.ToString("00.00", CultureInfo.InvariantCulture).Replace('.', ':');
                }
                else
                {
                    _scoreText = "--:--";
                }
            }

            public override void Draw(GraphicsContext graphicsContext)
            {
                Color color = this.Enabled ? Color.White : Color.Gray;

                RectangleF area = _area;
                float scaleMultiplier = this.IsPressedDown ? 0.9f : 1f;
                area = area.Inflate(scaleMultiplier == 1 ? 0 : -4, scaleMultiplier == 1 ? 0 : -4);

                // Outlines
                graphicsContext.PrimitiveRenderer.DrawRectangleOutlines(graphicsContext, area, color);

                // Text
                const string TextFontName = "Minecraftia18";
                graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[TextFontName], _text, area.Center, color, 0, scaleMultiplier);

                // Score
                const string ScoreFontName = "Minecraftia18";
                graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[ScoreFontName], _scoreText, new Vector2(area.Center.X, area.Bottom + 24), color, 0f, scaleMultiplier);
            }
        }
    }
}
