using LillyScan.FrontendXamarin.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LillyScan.FrontendXamarin.Views.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PreviewLinePredictionList : ContentView
	{
		public PreviewLinePredictionList()
		{
			InitializeComponent();			
			ListView.ItemsSource = Items;
		}		
		private readonly ObservableCollection<PreviewLinePrediction> Items = new ObservableCollection<PreviewLinePrediction>();

		public void AddItem(PreviewLinePrediction item)
		{
			Debug.WriteLine($"[PreviewLinePredictionList] Added item");
			Items.Add(item);			
		}

	}
}