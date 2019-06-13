using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtract
{
    class FormEntity
    {
        public string TXTPath {
            get;
            set;
        }
        public string Workspace
        {
            get;
            set;
        }

        public string OutFolder
        {
            get;
            set;
        }
        public string OutFolderAdd
        {
            get;
            set;
        }
        public string ReplaceSource
        {
            get;
            set;
        }
        public string ReplaceTarget
        {
            get;
            set;
        }
        public Boolean GainClass
        {
            get;
            set;
        }
        public Boolean RetainDirectory
        {
            get;
            set;
        }
        public Boolean GainTarGz { get; set; }
        public void processPathOperation()
        {
            Workspace = pathAddSeparateEnd(Workspace);
            ReplaceSource = pathAddSeparateEnd(ReplaceSource);
            ReplaceTarget = pathAddSeparateEnd(ReplaceTarget);
        }
        //格式化路径如：/hxbroot/WEB-INF/classes/  to：hxbroot\WEB-INF\classes\
        public string pathAddSeparateEnd(string path)
        {
            path = path.Replace("/", "\\");
            if (path.StartsWith("\\"))
            {
                path = path.Substring(1, path.Length - 1);
            }
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            return path;
        }

    }
}
