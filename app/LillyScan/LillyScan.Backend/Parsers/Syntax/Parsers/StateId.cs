namespace EsotericDevZone.Parsers.Syntax.Parsers
{
    public struct StateId : IParserStackElement
    {
        public int Id { get; }

        public StateId(int id)
        {
            Id = id;
        }

        public override string ToString() => $"${Id}";
    }
}
