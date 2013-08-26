using System.IO;
using Flai;

namespace WVVW.Editor.Model
{
    public class EditorWorld
    {
        private readonly EditorMap _editorMap;
        public EditorMap Map
        {
            get { return _editorMap; }
        }

        public bool IsModified { get; set; }

        public EditorWorld(EditorMap editorMap)
        {
            _editorMap = editorMap;
        }

        // probably not necessary
        public void Update(UpdateContext updateContext)
        {
        }

        public void Save(BinaryWriter writer)
        {
            _editorMap.Save(writer);
        }

        public bool IsPlayable(out string error)
        {
            return _editorMap.IsPlayable(out error);
        }

        public static EditorWorld Generate()
        {
            return new EditorWorld(EditorMap.Generate());
        }

        public static EditorWorld Load(BinaryReader reader)
        {
            return new EditorWorld(EditorMap.Load(reader));
        }
    }
}
