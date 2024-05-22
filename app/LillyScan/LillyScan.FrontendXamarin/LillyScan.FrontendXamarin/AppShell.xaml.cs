using LillyScan.FrontendXamarin.ViewModels;
using LillyScan.FrontendXamarin.Views;
using LillyScan.FrontendXamarin.Views.Pages;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ProcessingPage), typeof(ProcessingPage));
            Routing.RegisterRoute(nameof(ViewPredictionPage), typeof(ViewPredictionPage));
           
        }

        

    }
}
