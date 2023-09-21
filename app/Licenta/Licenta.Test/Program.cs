using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IWriteMatrix<string> m = new Matrix<IComparable>(2, 2);
            m[0, 0] = "123";
           
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
