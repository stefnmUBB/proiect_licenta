namespace LillyScan.Backend.Parsers.Lexic
{
    public class LexicalToken
    {
        public string Key { get; }
        public string Value { get; }
        public int Position { get; }

        public LexicalToken(string key, string value, int position)
        {
            Key = key;
            Value = value;
            Position = position;
        }

        public override string ToString()
        {
            return $"{Key}@{Position}: \"{Value}\"";
        }
    }
}
