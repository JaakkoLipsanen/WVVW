using System.IO;
using System.Windows.Forms;
using Flai;
using Flai.Extensions;
using Flai.Graphics;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WVVW.Editor;
using WVVW.Editor.Controller;
using WVVW.Editor.Model;
using WVVW.Editor.Ui;
using WVVW.Editor.View;
using WVVW.Screens;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

namespace WVVW
{
    public class EditorScreen : GameScreen
    {
        private readonly BasicUiContainer _uiContainer = new BasicUiContainer();
        private EditorLevel _level;
        private EditorLevelRenderer _levelRenderer;
        private EditorLevelController _levelController;
        private EditorFile _editorFile;
        private int _framesSinceDialog = 0;

        public EditorScreen()
            : this(null)
        {
        }

        public EditorScreen(string levelPath)
        {
            if (!string.IsNullOrEmpty(levelPath))
            {
                _editorFile = new EditorFile(Path.GetDirectoryName(levelPath), Path.GetFileName(levelPath));
                this.LoadLevel(true);
            }
            else
            {
                _editorFile = new EditorFile();

                _level = EditorLevel.Generate();
                _levelRenderer = new EditorLevelRenderer(_level);
                _levelController = new EditorLevelController(_level);
            }
        }

        protected override void LoadContent()
        {
            base.Game.ChangeResolution(EditorGlobals.EditorScreenWidth, EditorGlobals.EditorScreenHeight);

            _levelRenderer.LoadContent();
            this.CreateUi();
        }

        protected override void UnloadContent()
        {
            _levelRenderer.Unload();
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (this.IsActive)
            {
                _framesSinceDialog++;
                if (_framesSinceDialog > 15 && updateContext.InputState.IsNewKeyPress(XnaKeys.Escape))
                {
                    if ( _editorFile.NeedsSaving)
                    {
                        if (_editorFile.CanBeSaved)
                        {
                            this.SaveLevel();
                        }
                        else if (_uiContainer.FindFirst<SaveOpenPlayButtons>().Save())
                        {
                            // ignore
                        }
                        else
                        {
                            return;
                        }
                    }

                    LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
                }

                _uiContainer.Update(updateContext);

                _level.Update(updateContext);
                _levelController.Control(updateContext);
                _levelRenderer.Update(updateContext);

                _editorFile.NeedsSaving = _level.World.IsModified;
            }
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            _levelRenderer.Draw(graphicsContext);

            graphicsContext.SpriteBatch.Begin(SamplerState.PointClamp);
            _uiContainer.Draw(graphicsContext, true);
            graphicsContext.SpriteBatch.End();

            // Draw the file name
            const string FontName = "Minecraftia18";
            SpriteFont font = graphicsContext.FontContainer[FontName];

            if (_editorFile.Name != null)
            {
                graphicsContext.SpriteBatch.Begin();
                graphicsContext.SpriteBatch.DrawString(font, _editorFile.Name, new Vector2(EditorGlobals.EditorScreenWidth - font.GetStringWidth(_editorFile.Name), EditorGlobals.EditorScreenHeight - 40), Color.White);
                graphicsContext.SpriteBatch.End();
            }
        }

        private void CreateUi()
        {
            _uiContainer.Clear();

            SaveOpenPlayButtons saveOpenPlayButtons = new SaveOpenPlayButtons(_editorFile);
            _uiContainer.Add(saveOpenPlayButtons);
            _uiContainer.Add(new TileSelectorList(_level.Player));
            _uiContainer.Add(new ResizeButton(_level.Player));

            saveOpenPlayButtons.TryRequested += () =>
            {
                string error = "";
                if (!_editorFile.NeedsSaving && _level.IsPlayable(out error))
                {
                    // Okay we are now loading the whole stuff
                    LoadingScreen.Load(base.ScreenManager, false, new GameplayScreen(_editorFile.FullPath, GameplayMode.Editor));
                }
                else
                {
                    MessageBox.Show("Error when trying to play the level: " + (error ?? ""));
                }
            };

            saveOpenPlayButtons.LoadRequested += (levelPath) =>
            {
                this.LoadLevel(true);
            };

            saveOpenPlayButtons.SaveRequested += () =>
            {
                this.SaveLevel();
            };

            saveOpenPlayButtons.NewLevelRequested += () =>
            {
                this.LoadLevel(false);
            };

            saveOpenPlayButtons.DialogOpened += () =>
            {
                _framesSinceDialog = 0;
            };
        }

        private void SaveLevel()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(_editorFile.FullPath, FileMode.Create, FileAccess.Write)))
            {
                _level.Save(writer);
            }

            _editorFile.NeedsSaving = false;
            _level.World.IsModified = false;
        }

        private void LoadLevel(bool loadFromFile = true)
        {
            if (_levelRenderer != null)
            {
                _levelRenderer.Unload();
            }

            if (loadFromFile)
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(_editorFile.FullPath)))
                {
                    _level = EditorLevel.Load(reader);
                }
            }
            else
            {
                _level = EditorLevel.Generate();
            }

            _levelRenderer = new EditorLevelRenderer(_level);
            _levelController = new EditorLevelController(_level);

            if (base.IsLoaded)
            {
                _levelRenderer.LoadContent();
                this.CreateUi();
            }
        }
    }
}
