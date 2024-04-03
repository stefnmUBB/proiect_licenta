using Camera.MAUI;
using LillyScan.Backend;
using LillyScan.Backend.API;
using LillyScan.Backend.Imaging;
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


        DefaultHTREngine API; //= new DefaultHTREngine();


        private bool IsLiveSegmentationActive = true;
        private Task LiveSegmentationTask;

        private async void LiveSegmentation()
        {
            Thread.Sleep(5000);
            while (IsLiveSegmentationActive)
            {
                Thread.Sleep(200);
                if (cameraView == null) continue;
                lock (cameraView)
                    if (!cameraView.AreCamerasLoaded) continue;
                var pic = cameraView.GetSnapshot() as StreamImageSource;
                if (pic == null) continue;
                using var stream = await pic.Stream(CancellationToken.None);
                using (var image = PlatformImage.FromStream(stream))
                {
                    Console.WriteLine($"Image {image.Width}x{image.Height}");

                    using var cropped = image.CropCentered(70, 70);
                    using var resized = cropped.Resize(256, 256, ResizeMode.Stretch);

                    Console.WriteLine($"Resized");
                    var imageRGB = resized.ToBytesRGB();

                    if (imageRGB == null) continue;

                    try
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        var segm = API.Segment(imageRGB);
                        sw.Stop();
                        Console.WriteLine("________________________________________________________");
                        Console.WriteLine($"Elapsed {sw.Elapsed} | {sw.ElapsedMilliseconds}ms");
                    }
                    catch (AggregateException e)
                    {
                        foreach (var ex in e.Flatten().InnerExceptions)
                        {
                            Console.WriteLine(ex.GetType());
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        private void MainPage_Loaded(object sender, EventArgs e)
        {
            LiveSegmentationTask = Task.Run(LiveSegmentation);
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            //cameraView.CaptureAsync()

            /*if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);*/
        }
    }

}
