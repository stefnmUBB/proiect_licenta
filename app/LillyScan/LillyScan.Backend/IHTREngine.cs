﻿using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;

namespace LillyScan.Backend
{
    public interface IHTREngine
    {
        byte[] Segment(byte[] image);
        string Predict(IReadMatrix<double> image);
        string Predict(ImageRGB image);
    }
}
