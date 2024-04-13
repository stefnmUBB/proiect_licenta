using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.Backend.Math.Arithmetics.BuiltInTypeWrappers;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LillyScan.Backend.API
{
    public class HTRClient : HTREngine
    {
        private readonly string Uri;
        HttpMessageHandler MessageHandler = null;
        HttpClient client;

        public HTRClient(string uri = "http://localhost:8000/", HttpMessageHandler messageHandler = null)
        {
            Uri = uri;
            MessageHandler = messageHandler;
            client = new HttpClient(MessageHandler);
        }

        public override string Predict(IReadMatrix<double> image)
        {            
            throw new NotImplementedException();
        }

        public override float[] Segment(float[] image) { throw new NotImplementedException(); }

        public override byte[] Segment(byte[] image)
        {            
            var enc = Base64.Encode(image);
            
            //client.BaseAddress = new Uri(Uri);
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            Console.WriteLine("[API] Created client");

            string recvData = "";
            Task.Run(async () =>
            {
                Console.WriteLine("[API] Posting");
                var content = new StringContent($"{{\"image\":\"{enc}\"}}");
                Console.WriteLine($"[API] {Uri}/seg");
                HttpResponseMessage response = await client.PostAsync($"{Uri}/seg", content);
                Console.WriteLine("[API] RECV??");
                response.EnsureSuccessStatusCode();
                Console.WriteLine("[API] Reading response");
                recvData = await response.Content.ReadAsStringAsync();
            }).Wait();
            Console.WriteLine("[API] Decoding");
            return Base64.Decode(recvData);            
        }

        public override float[] Segment64(float[] image, bool preview = false)
        {
            throw new NotImplementedException();
        }
    }
}
