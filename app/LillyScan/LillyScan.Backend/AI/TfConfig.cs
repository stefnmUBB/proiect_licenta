using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI
{
    public class TfConfig
    {
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public TfConfig(string config)
        {

        }

    }
}
