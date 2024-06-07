using LillyScan.Backend.AI.Models;

namespace LillyScan.LSMConv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("No input");
                Environment.Exit(0);
            }
            var inFile = args[0];

            try
            {
                using (var f = File.OpenRead(inFile))
                {
                    var bytes = ModelLoader.StreamToBytes(f);
                    var outFile = inFile + ".lsm";
                    if (args.Length == 2)
                        outFile = args[1];
                    File.WriteAllBytes(outFile, bytes);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
                Environment.Exit(-1);
            }

            
        }
    }
}
