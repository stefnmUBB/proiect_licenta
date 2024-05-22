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

        public App()
        {
            InitializeComponent();
            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
            PredictionRepository = new PredictionRepository(Path.Combine(FileSystem.AppDataDirectory, "predictions"));
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
