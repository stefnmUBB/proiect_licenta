using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BlitzToggle : ContentView
    {
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(BlitzToggle), false, BindingMode.TwoWay,
            propertyChanged: IsCheckedPropertyChanged);

        private static void IsCheckedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as BlitzToggle;
        }

        public BlitzToggle()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            IsChecked ^= true;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        public delegate void OnCheckedChanged(object sender, EventArgs e);
        public event OnCheckedChanged CheckedChanged;
    }
}