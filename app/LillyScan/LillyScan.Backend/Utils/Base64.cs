using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LillyScan.Backend.Utils
{
    public static class Base64
    {
        public static string Encode(byte[] bytes)
        {            
            return System.Convert.ToBase64String(bytes);
        }

        public static byte[] Decode(string base64EncodedData)
        {
            return System.Convert.FromBase64String(base64EncodedData);            
        }

    }
}
