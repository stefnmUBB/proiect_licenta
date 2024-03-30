using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    public static class LayerConfigConverter
    {
        public static TfConfig Convert(Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("config"))
                throw new InvalidOperationException("Cannot create LSTM layer from dictionary");

            if(!(dict["config"] is Dictionary<string, object> cfg))
                throw new InvalidOperationException("Invalid `config` property");
            return new TfConfig(cfg); 
        }
    }
}
