using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace LillyScan.FrontendXamarin.Models
{    
    public class CaptureBytes : INotifyPropertyChanged
    {
        public byte[] Bytes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
