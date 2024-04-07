using Camera.MAUI;
using LillyScan.Backend;
using LillyScan.Backend.API;
using LillyScan.Backend.Imaging;
using LillyScan.Backend.MAUI.Imaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics.Platform;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace LillyScan.Frontend
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        static void Measure(string message, Action a)
        {
            Console.WriteLine($"Starting {message}");
            var sw = new Stopwatch();
            sw.Start();
            a();
            sw.Stop();
            Console.WriteLine($"Finished {message}: {sw.Elapsed} | {sw.ElapsedMilliseconds}ms");
        }

        public MainPage()
        {            
            InitializeComponent();

            Console.WriteLine("HERE!!!!");
            Measure("DefaultHTREngine", () =>
            {
                try
                {
                    API = new DefaultHTREngine();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"e: {e.GetType()}: {e.Message} | {e.StackTrace}");
                    e = e.InnerException;
                    if (e == null) return;
                    Console.WriteLine($"i: {e.GetType()}: {e.Message} | {e.StackTrace}");
                }
            });
            Loaded += MainPage_Loaded;            
        }


        DefaultHTREngine API;


        private bool IsLiveSegmentationActive = true;
        private Task LiveSegmentationTask;

        private async void LiveSegmentation()
        {
            var r = new Random();
            Microsoft.Maui.Graphics.IImage segmImage = null;
            Thread.Sleep(5000);
            string elapsed = "";
            int k = 0;
            while (IsLiveSegmentationActive)
            {
                
                Thread.Sleep(200);
                if (cameraView == null) continue;
                var img = cameraView.GetSnapshot();
                
                //(img as StreamImageSource)
                /*if (img == null) continue;

                Console.WriteLine($"Image {img.Width}x{img.Height}");

                var cropped = img.CropCenteredPercent(70, 70, true);
                var resized = cropped.Resize(256, 256, true);
                var gray = resized.AverageChannels(true);
                Console.WriteLine($"Resized");                                

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var drw = GraphicsView2.Drawable as MyDrawable;
                    drw.Color = new Color(r.Next() % 255, 0, 0);
                    drw.Image = segmImage;
                    GraphicsView2.Invalidate();
                });

                segmImage = null;
                try
                {                    
                    var sw = new Stopwatch();
                    sw.Start();
                    var segm = API.SelectTiled64(gray, parallel: true);                                        
                    sw.Stop();                    
                    Console.WriteLine("________________________________________________________");
                    Console.WriteLine($"Elapsed {sw.Elapsed} | {sw.ElapsedMilliseconds}ms");
                    segmImage = segm.ToImage();                    
                }
                catch (AggregateException e)
                {
                    foreach (var ex in e.Flatten().InnerExceptions)
                    {
                        Console.WriteLine(ex.GetType());
                        Console.WriteLine(ex.Message);
                    }
                }


                gray.Dispose();*/

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Title = $"{DateTime.Now.Minute}:{DateTime.Now.Second} => {elapsed} ({++k})";
                });
            }
        }

        private void MainPage_Loaded(object sender, EventArgs e)
        {
            LiveSegmentationTask = Task.Run(LiveSegmentation);
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;           
        }
    }

}
