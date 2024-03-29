using System;
using System.Runtime.Serialization;

namespace EsotericDevZone.Parsers.Syntax.Parsers
{
    [Serializable]
    internal class ConflictGrammarException : Exception
    {
        public ConflictGrammarException()
        {
        }

        public ConflictGrammarException(string message) : base(message)
        {
        }

        public ConflictGrammarException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConflictGrammarException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}