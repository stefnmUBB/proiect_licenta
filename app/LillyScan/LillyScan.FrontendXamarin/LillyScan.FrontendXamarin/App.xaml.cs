﻿using Android.OS;
using LillyScan.FrontendXamarin.Services;
using LillyScan.FrontendXamarin.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin
{
    public partial class App : Application
    {

        public App()
        {
            StrictMode.EnableDefaults();

            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
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
