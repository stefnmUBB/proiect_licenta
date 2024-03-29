using System;

namespace LillyScan.Backend.Types
{
    public class NamedAttribute : Attribute
    {
        public string Name { get; }

        public NamedAttribute(string name) 
        {
            Name = name;
        }
    }
}
