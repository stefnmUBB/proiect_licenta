using LillyScan.FrontendXamarin.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
		public int ItemsCount => Items.Count;

		public List<PreviewLinePrediction> GetItems() => Items.ToList();

		public void ForeachItem(Action<PreviewLinePrediction> action)
		{
			for(int i=0, l=Items.Count;i<l;i++)			
			{
				action(Items[i]);
			}

			/*var partition = Partitioner.Create(0, Items.Count, Items.Count / 4);
			Parallel.ForEach(partition, new ParallelOptions { MaxDegreeOfParallelism = 4 }, range =>
			{
				for (int i = range.Item1; i < range.Item2; i++)
					action(Items[i]);

			});*/			
		}

		public void BeginRefresh() => ListView.BeginRefresh();
		public void EndRefresh() => ListView.EndRefresh();


		public void Clear()
		{
			Items.Clear();
		}
		public void RefreshItem(PreviewLinePrediction item)
		{
			int index = Items.IndexOf(item);
			if (index < 0) return;
			Items[index] = item;			
		}

	}
}