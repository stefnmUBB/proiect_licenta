using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LillyScan.Backend.AI.Models
{
    public static class ModelLoader
    {
        private static string[] LayerAttributes = new[]
        {
            "[type]", "[]"
        };

        public static void LoadFromStream(Stream stream)
        {
            using(TextReader r =new StreamReader(stream))
            {
                while(stream.CanRead)
                {
                    var line = r.ReadLine().Trim();
                    if(line == "[[Layer]]")
                    {
                        
                    }
                }
            }
        }

    }
}
