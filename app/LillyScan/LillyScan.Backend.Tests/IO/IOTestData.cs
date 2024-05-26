namespace LillyScan.Backend.Tests.IO
{
    public class IOTestData
    {
        public readonly string Input;
        public readonly string Output;

        public IOTestData(string input, string output)
        {
            Input = input;
            Output = output;
        }

        public static IOTestData FromTestFiles(string name)
        {
            var input = File.ReadAllText(name + ".in");
            var output = File.ReadAllText(name + ".out");
            return new IOTestData(input, output);
        }
    }
}
