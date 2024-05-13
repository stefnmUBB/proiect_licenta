using System;

namespace LillyScan.Backend.HTR
{
    [Flags]
    public enum SegmentationType
    {
        Padded=0x1,
        Normal=0x2,
        Preview=0x4,
        Linear=0x8,
        PaddedLinear = Padded | Linear,
    }
}
