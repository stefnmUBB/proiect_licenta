using System.Collections.Generic;

namespace LillyScan.Backend.Parsers
{
    internal static class Validators
    {
        public static List<T> RequireNotEmpty<T>(List<T> list, string listName = "List")
        {
            if (list == null)
                throw new ValidationException($"{listName} cannot be null.");
            if (list.Count == 0)
                throw new ValidationException($"{listName} cannot be empty");
            return list;
        }

        public static void Assert(string message, bool condition)
        {
            if (!condition)
                throw new ValidationException($"Assertion failed: {message}");
        }
    }
}
