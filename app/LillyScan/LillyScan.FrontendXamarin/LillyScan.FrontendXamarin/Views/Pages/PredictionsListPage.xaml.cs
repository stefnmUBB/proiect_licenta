using LillyScan.FrontendXamarin.ViewModels.Pages;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PredictionsListPage : ContentPage
    {        
        public PredictionsListPage()
        {            
            InitializeComponent();
            BindingContext = new PredictionsListViewModel(this);
        }       
    }
}
