using LillyScan.Backend.Math;
using System;
using System.Runtime.Serialization;

namespace LillyScan.Backend.AI.Layers
{
    [Serializable]
    internal class ShapeMismatchException : Exception
    {
        public Shape Placeholder;
        public Shape Real;    

        public ShapeMismatchException(Shape placeholder, Shape real) 
            : base($"Invalid shape: expected {placeholder}, got {real}")
        {
            Placeholder = placeholder;
            Real = real;
        }        
    }
}