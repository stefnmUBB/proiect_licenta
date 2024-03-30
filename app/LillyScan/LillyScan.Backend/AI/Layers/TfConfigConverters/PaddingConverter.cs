namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    internal static class PaddingConverter
    {
        public static Padding Convert(string input)
        {
            input = input.ToLower();
            if (input == "valid") return Padding.Valid;
            if (input == "same") return Padding.Same;
            throw new TfConfigConverterFailedException(input, typeof(Padding));
        }
    }
}
