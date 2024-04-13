using LillyScan.FrontendXamarin.ViewModels;
using LillyScan.FrontendXamarin.Views;
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
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));            
        }

        

    }
}
