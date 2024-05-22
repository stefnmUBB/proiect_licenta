using DLToolkit.Forms.Controls;
using LillyScan.FrontendXamarin.Repository;
using LillyScan.FrontendXamarin.Services;
using LillyScan.FrontendXamarin.Views;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin
{
    public partial class App : Application
    {
        public PredictionRepository PredictionRepository { get; }
        public ImageStorage ImageStorage { get; }

        public App()
        {
            InitializeComponent();
            DependencyService.Register<MockDataStore>();
            FlowListView.Init();

            //ClearData();

            MainPage = new AppShell();
            PredictionRepository = new PredictionRepository(Path.Combine(FileSystem.AppDataDirectory, "predictions"));
            ImageStorage = new ImageStorage(Path.Combine(FileSystem.AppDataDirectory, "images"));
        }

        private void ClearData()
        {
            Directory.Delete(Path.Combine(FileSystem.AppDataDirectory, "predictions"), true);
            Directory.Delete(Path.Combine(FileSystem.AppDataDirectory, "images"), true);

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
