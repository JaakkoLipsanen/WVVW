using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Flai;
using Flai.Graphics;
using Flai.Ui;
using Microsoft.Xna.Framework;
using WVVW.Editor.Model;

namespace WVVW.Editor.Ui
{
    public class SaveOpenPlayButtons : UiObject
    {
        private readonly EditorFile _file;

        public event GenericEvent DialogOpened;
        public event GenericEvent SaveRequested;
        public event GenericEvent<string> LoadRequested;
        public event GenericEvent TryRequested;
        public event GenericEvent NewLevelRequested;

        public SaveOpenPlayButtons(EditorFile editorFile)
        {
            _file = editorFile;
            this.CreateUi();
        }

        public override void Update(UpdateContext updateContext)
        {
            base.Children.Update(updateContext);
        }

        public override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, new Rectangle(0, EditorGlobals.EditorScreenHeight - 40, 480, 40), Color.Black * 0.5f);
            base.Children.Draw(graphicsContext, true);
        }

        private void CreateUi()
        {
            base.Children.Clear();

            const string FontName = "Minecraftia18";
            EditorButton tryButton = new EditorButton("Try", new Vector2(128, EditorGlobals.EditorScreenHeight - 24)) { Font = FontName };
            tryButton.Click += (o, e) =>
            {
                if (_file.NeedsSaving)
                {
                    if (!this.Save())
                    {
                        return;
                    }
                }

                this.TryRequested.InvokeIfNotNull();
            };
            base.Children.Add(tryButton);

            EditorButton saveButton = new EditorButton("Save", new Vector2(228, EditorGlobals.EditorScreenHeight - 24)) { Font = FontName };
            saveButton.Click += (o, e) =>
            {
                this.Save();
            };
            base.Children.Add(saveButton);

            EditorButton openButton = new EditorButton("Open", new Vector2(344, EditorGlobals.EditorScreenHeight - 24)) { Font = FontName };
            openButton.Click += (o, e) =>
            {
                if (_file.NeedsSaving)
                {
                    if (!this.Save())
                    {
                        return;
                    }
                }

                this.Open();
            };
            base.Children.Add(openButton);

            EditorButton newButton = new EditorButton("New", new Vector2(444, EditorGlobals.EditorScreenHeight - 24)) { Font = FontName };
            newButton.Click += (o, e) =>
            {
                if (_file.NeedsSaving)
                {
                    this.Save();
                }

                this.NewLevelRequested.InvokeIfNotNull();
            };
            base.Children.Add(newButton);
        }

        public bool Save()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                OverwritePrompt = true,
                DefaultExt = WvvwGlobals.FileExtension,
                Filter = string.Format("WVVW Levels(*{0})|*{0}", WvvwGlobals.FileExtension),
            };

            if (!string.IsNullOrEmpty(_file.Path) && Directory.Exists(_file.Path))
            {
                saveFileDialog.InitialDirectory = _file.Path;
                if (!string.IsNullOrEmpty(_file.Name) && File.Exists(_file.FullPath))
                {
                    saveFileDialog.FileName = _file.Name;
                }
            }

            DialogResult result = saveFileDialog.ShowDialog();
            this.DialogOpened.InvokeIfNotNull();
            if (result == DialogResult.OK)
            {
                _file.Path = Path.GetDirectoryName(saveFileDialog.FileName);
                _file.Name = Path.GetFileName(saveFileDialog.FileName);

                this.SaveRequested.InvokeIfNotNull();
                return true;
            }

            return false;
        }

        private void Open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                CheckFileExists = true,
                DefaultExt = WvvwGlobals.FileExtension,
                Filter = string.Format("WVVW Levels(*{0})|*{0}", WvvwGlobals.FileExtension),
            };

            if (!string.IsNullOrEmpty(_file.Path) && Directory.Exists(_file.Path))
            {
                openFileDialog.InitialDirectory = _file.Path;
                if (!string.IsNullOrEmpty(_file.Name) && File.Exists(_file.FullPath))
                {
                    openFileDialog.FileName = _file.Name;
                }
            }

            DialogResult result = openFileDialog.ShowDialog();
            this.DialogOpened.InvokeIfNotNull();
            if (result == DialogResult.OK)
            {
                _file.Path = Path.GetDirectoryName(openFileDialog.FileName);
                _file.Name = Path.GetFileName(openFileDialog.FileName);

                this.LoadRequested.InvokeIfNotNull(openFileDialog.FileName);
            }
        }

        #region Editor Button

        private class EditorButton : TextButton
        {
            public EditorButton(string text, Vector2 position)
                : base(text, position)
            {
            }
        }

        #endregion
    }
}
