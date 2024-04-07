using System;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CapturePage : ContentPage
    {        

        public CapturePage()
        {
            InitializeComponent();
            //LiveSegmentationPreviewTask = Task.Run(LiveSegmentationPreview);
        }

        Task LiveSegmentationPreviewTask;
        private bool _LiveSegmentationPreviewTaskRunning = true;
        private bool LiveSegmentationPreviewTaskRunning
        {
            get { lock (this) return _LiveSegmentationPreviewTaskRunning;  }
            set { lock (this) _LiveSegmentationPreviewTaskRunning = value; }
        }

        private async void LiveSegmentationPreview()
        {
            //Thread.Sleep(5000);
            Console.WriteLine($"[LiveSegmentationPreview] Started.");

            //CameraPreview.Refresh();
            while (LiveSegmentationPreviewTaskRunning)
            {
                Thread.Sleep(50);
                using (var bmp = await CameraPreview.TakePicture())
                {
                    //Console.WriteLine("Here??");

                }
                //Console.WriteLine("Out??");
            }
            Console.WriteLine($"[LiveSegmentationPreview] Finished.");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            Console.WriteLine("[CapturePage] OnAppearing");

            if (!LiveSegmentationPreviewTask?.IsCompleted ?? false)
            {
                Console.WriteLine("[CapturePage] Waiting for completion");
                LiveSegmentationPreviewTaskRunning = false;
                LiveSegmentationPreviewTask.Wait();
            }            
            LiveSegmentationPreviewTaskRunning = true;            
            Console.WriteLine("[CapturePage] Starting live preview");
            LiveSegmentationPreviewTask?.Dispose();
            LiveSegmentationPreviewTask = Task.Run(LiveSegmentationPreview);            
        }

        protected override void OnDisappearing()
        {            
            base.OnDisappearing();
            Console.WriteLine("[CapturePage] OnDisappearing");
            LiveSegmentationPreviewTaskRunning = false;
            if (!LiveSegmentationPreviewTask?.IsCompleted ?? false) 
            {
                Console.WriteLine("[CapturePage] Closing live preview");
                LiveSegmentationPreviewTask?.Wait();
                LiveSegmentationPreviewTask?.Dispose();
            }
            else
            {
                Console.WriteLine("[CapturePage] Live preview was not running");
            }
        }

        ~CapturePage()
        {
            LiveSegmentationPreviewTaskRunning = false;
        }
    }
}