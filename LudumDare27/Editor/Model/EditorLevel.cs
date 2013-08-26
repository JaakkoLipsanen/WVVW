using System.IO;
using Flai;

namespace WVVW.Editor.Model
{
    public class EditorLevel
    {
        private readonly EditorWorld _world;
        private readonly EditorPlayer _player;

        public EditorWorld World
        {
            get { return _world; }
        }

        public EditorPlayer Player
        {
            get { return _player; }
        }

        public EditorLevel(EditorWorld world)
        {
            _world = world;
            _player = new EditorPlayer();
        }

        public void Update(UpdateContext updateContext)
        {
            _world.Update(updateContext);
        }

        public bool IsPlayable(out string error)
        {
            return _world.IsPlayable(out error);
        }

        public void Save(BinaryWriter writer)
        {
            _world.Save(writer);
        }

        public static EditorLevel Generate()
        {
            return new EditorLevel(EditorWorld.Generate());
        }

        public static EditorLevel Load(BinaryReader reader)
        {
            return new EditorLevel(EditorWorld.Load(reader));
        }

        
    }
}
