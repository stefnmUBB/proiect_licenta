using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Parsers
{
    public class ParseResult<T>
    {
        public string Type { get; }
        public T Value { get; }
        public class Success : ParseResult<T>
        {
            public Success(string type, T value) : base(type, value) { }

            public override string ToString()
            {
                return $"ParseResult({Type}, Success){{ {Value} }}";
            }
        }

        public class Error : ParseResult<T>
        {
            public string Message { get; }
            public int Position { get; }

            public Error(string type, T value, string message, int position) : base(type, value)
            {
                Message = message;
                Position = position;
            }

            public override string ToString()
            {
                return $"ParseResult({Type}, Error at {Position}: {Message}){{ {Value} }}";
            }
        }


        protected ParseResult(string type, T value)
        {
            Type = type;
            Value = value;
        }
    }
}
