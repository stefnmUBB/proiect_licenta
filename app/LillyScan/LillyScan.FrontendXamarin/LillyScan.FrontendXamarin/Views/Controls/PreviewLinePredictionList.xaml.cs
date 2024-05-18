using LillyScan.FrontendXamarin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

			for(int i=0;i<3;i++)
			{
				Items.Add(new PreviewLinePrediction { PredictedText = $"This is line {i + 1}" });
            }			
			ListView.ItemsSource = Items;
		}

		private IList<PreviewLinePrediction> Items = new List<PreviewLinePrediction>();



	}
}