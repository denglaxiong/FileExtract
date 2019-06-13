using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExtract
{
    [Serializable]
    class FormConfEntity
    {
        public List<String> TXTPath{
            get;
            set;
        }
        public List<String> Workspace
        {
            get;
            set;
        }

        public  List<string> OutFolder
        {
            get;
            set;
        }
        public List<String> OutFolderAdd
        {
            get;
            set;
        }
        public List<string> ReplaceSource
        {
            get;
            set;
        }
        public List<string> ReplaceTarget
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

    }
}
