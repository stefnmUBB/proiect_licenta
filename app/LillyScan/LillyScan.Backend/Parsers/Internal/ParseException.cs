using System;
using System.Runtime.Serialization;

namespace LillyScan.Backend.Parsers.Internal
{
    [Serializable]
    internal class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string type, string message, int position) : base($"{type} error at {position}: {message}")
        {
        }      


    }
}