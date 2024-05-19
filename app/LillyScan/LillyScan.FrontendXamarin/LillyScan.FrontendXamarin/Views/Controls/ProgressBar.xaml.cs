using LillyScan.FrontendXamarin.Utils;
using LillyScan.FrontendXamarin.Views.Pages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgressBar : ContentView
    {
        public ProgressBar()
        {
            InitializeComponent();
        }

        private double pPercentage = 0;
        public double Percentage
        {
            get => pPercentage;
            set
            {
                pPercentage = value;
                FormattedPercentage = $"{pPercentage:0.00}%";
                FormattedBarLayoutBounds = new Rectangle(0, 0, pPercentage * 0.01, 1);
            }
        }


        public string FormattedPercentage
        {
            get => GetValue(FormattedPercentageProperty) as string;
            set => SetValue(FormattedPercentageProperty, value);
        }
        public static readonly BindableProperty FormattedPercentageProperty = BindableProperty
            .Create(nameof(FormattedPercentage), typeof(string), typeof(ProgressBar), "0.00%");
        
        public Rectangle FormattedBarLayoutBounds
        {
            get => (Rectangle)GetValue(FormattedBarLayoutBoundsProperty);
            set => SetValue(FormattedBarLayoutBoundsProperty, value);
        }

        public static readonly BindableProperty FormattedBarLayoutBoundsProperty = BindableProperty
            .Create(nameof(FormattedBarLayoutBounds), typeof(Rectangle), typeof(ProgressBar), new Rectangle(0, 0, 0, 1));
    }
}