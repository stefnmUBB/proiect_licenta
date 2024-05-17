using LillyScan.FrontendXamarin.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.Converters
{
    class ProcessingStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("Convert called!!!");
            Debug.WriteLine($"From {value} ({value?.GetType()}) to {targetType}");
            if (value is null)
                throw new ArgumentNullException("Expected ProcessingState, got null");
            if (!(value is ProcessingState state))
                throw new ArgumentException($"Expected ProcessingState, got {value.GetType()}");            
            if (targetType == typeof(string))
                return Enum.GetName(typeof(ProcessingState), state);
            if (targetType == typeof(int))
                return (int)state;
            throw new NotImplementedException($"Cannot convert ProcessingState to {targetType}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine("ConvertBack called!!!");
            if (value is null)
                throw new ArgumentNullException("Cannot get ProcessingState from null");
            if (targetType != typeof(ProcessingState))
                throw new InvalidOperationException($"Expected to convert to ProcessingState, found {targetType}");
            if (value is string s)
                return (ProcessingState)Enum.Parse(typeof(ProcessingState), s);
            if (value is int n)
                return (ProcessingState)n;
            throw new NotImplementedException($"Cannot convert {value.GetType()} to ProcessingState");
        }
    }
}
