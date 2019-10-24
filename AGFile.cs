using System;
using System.Collections.Generic;
using System.Text;

namespace Avantgarde.Lib
{
    public class AGFile
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public List<string> Tags { get; set; } // static, master, skipExisting, dontCreateTargetDir

        public AGFile(string source, string destination)
        {
            Source = source;
            Destination = destination;
            Tags = new List<string>();
        }

    }

}
