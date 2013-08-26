using System.IO;
using System.Windows.Forms;
using SysPath = System.IO.Path;

namespace WVVW.Editor.Model
{
    public class EditorFile
    {
        private string _name;
        private string _path;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string FullPath
        {
            get
            {
                if (_path == null)
                {
                    return null;
                }
                else if (_name == null)
                {
                    return _path;
                }

                return SysPath.Combine(_path, _name);
            }
        }

        public bool CanBeSaved
        {
            get
            {
                return !string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_path) && File.Exists(SysPath.Combine(_path, _name));
            }
        }

        public bool NeedsSaving
        {
            get;
            set;
        }

        public EditorFile()
            : this(SysPath.Combine(SysPath.GetDirectoryName(Application.ExecutablePath), "Levels"), null)
        {
        }

        public EditorFile(string path, string name)
        {
            _path = path;
            _name = name;
        }  
    }
}
