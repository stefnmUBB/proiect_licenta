using LillyScan.FrontendXamarin.Models;
using LillyScan.FrontendXamarin.Repository;
using LillyScan.FrontendXamarin.Views.Pages;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace LillyScan.FrontendXamarin.ViewModels.Pages
{
    internal class PredictionsListViewModel : BindableObject
    {
        private readonly PredictionsListPage Page;
        private readonly PredictionRepository Repository;      

        public class Item
        {
            public Prediction Prediction { get; set; }
            public ImageSource Thumbnail { get; set; }
        }

        public PredictionsListViewModel(PredictionsListPage page)
        {
            Page = page;
            Repository = (App.Current as App).PredictionRepository;            
            Repository.CollectionChanged += Repository_CollectionChanged;
            Debug.WriteLine("Loading items");
            Items = new ObservableCollection<Item>();
            Refresh();
            Debug.WriteLine("Loaded items");            
        }

        private void Refresh()
        {
            Items = new ObservableCollection<Item>(Repository.GetAll()
                .Select(p => new Item
                {
                    Prediction = p,
                    Thumbnail = ImageSource.FromStream(() => new MemoryStream(p.Image.Thumbnail))
                }));
        }

        private void Repository_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("Repository_CollectionChanged");
            Refresh();
        }

        private ObservableCollection<Item> _items = new ObservableCollection<Item>();
        public ObservableCollection<Item> Items
        {
            get => _items;            
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }
        public Command ItemTappedCommand
        {
            get => new Command(async (data) =>
            {
                
                await Page.DisplayAlert("FlowListView", data + "", "Ok");
            });            
        }        

    }
}
