using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Utils
{
    public interface ISequenceAccessor 
    { 
        bool DimReduce { get; }
        int[] GetIndices(int length);
    }   
}
