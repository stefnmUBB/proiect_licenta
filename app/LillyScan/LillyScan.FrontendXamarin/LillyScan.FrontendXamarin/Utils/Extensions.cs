using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Utils
{
    public static class Extensions
    {
        public static async Task<byte[]> ToBytes(this ImageSource imageSource)
        {
            if (imageSource is StreamImageSource sis)
            {
                using (var stream = await sis.Stream(CancellationToken.None))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            throw new ArgumentException($"StreamImageSource was expected, got {imageSource?.GetType()?.ToString() ?? "null"}");
        }

        public static A As<A>(this Application app) where A : Application => app as A;
    }
}
