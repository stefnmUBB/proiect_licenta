using LillyScan.FrontendXamarin.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}