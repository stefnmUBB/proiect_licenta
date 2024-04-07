using LillyScan.FrontendXamarin.Views.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            LiveSegmentationPreviewTask = Task.Run(LiveSegmentationPreview);
        }

        Task LiveSegmentationPreviewTask;
        private bool LiveSegmentationPreviewTaskRunning = true;

        private async void LiveSegmentationPreview()
        {
            //Thread.Sleep(5000);
            Console.WriteLine($"[LiveSegmentationPreview] Started.");
            while(LiveSegmentationPreviewTaskRunning)
            {
                Thread.Sleep(50);
                await CameraPreview.TakePicture();
                Console.WriteLine("Out??");
            }
            Console.WriteLine($"[LiveSegmentationPreview] Finished.");
        }

        ~AboutPage()
        {
            LiveSegmentationPreviewTaskRunning = false;
        }

    }
}