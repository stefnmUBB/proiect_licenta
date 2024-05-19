using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingSpinner : ContentView
    {
        public LoadingSpinner()
        {
            InitializeComponent();
            Spinner.AnchorX = Spinner.AnchorY = 0.5;
            new Animation(t => Spinner.Rotation = t, 0, 360)
                .Commit(Spinner, "SpinnerRotate", length: 1000, repeat: () => true);            
        }
    }
}