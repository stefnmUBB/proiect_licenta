using LillyScan.FrontendXamarin.Models;
using LillyScan.FrontendXamarin.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LillyScan.FrontendXamarin.Repository
{
    public class PredictionRepository
    {        
        private readonly string PredictionsFilename;
        private readonly string PredictedLinesDir;        
        private readonly HashSet<int> NeedsLineSave = new HashSet<int>();
        private readonly ObservableCollection<Prediction> Items;
        private bool IsLoaded = false;
        private int LastId;

        private Logger Log = Logger.Create<PredictionRepository>();

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => Items.CollectionChanged += value;
            remove => Items.CollectionChanged -= value;
        }

        public PredictionRepository(string path)
        {            
            PredictionsFilename = Path.Combine(path, "items.dat");
            PredictedLinesDir = Path.Combine(path, "lines");
            if (!Directory.Exists(PredictedLinesDir))
                Directory.CreateDirectory(PredictedLinesDir);
            Items = new ObservableCollection<Prediction>();
        }        

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ObservableCollection<Prediction> GetAll()
        {
            if (!IsLoaded)
            {
                IsLoaded = true;
                LoadFromStorage();                
            }
            return Items;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(Prediction pred)
        {
            Log?.WriteLine($"Add");
            pred.Id = LastId++;
            Items.Add(pred);            
            NeedsLineSave.Add(pred.Id);
            WriteToStorage();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Remove(Prediction pred)
        {
            foreach (var item in Items.Where(_ => _.Id == pred.Id).ToArray()) 
                Items.Remove(item);            
            WriteToStorage();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LoadPredictedLines(Prediction pred)
        {
            ReadPredictedLines(pred);
        }

        private void WriteToStorage()
        {
            Log?.WriteLine($"WriteToStorage {Items.Count}");
            using (var f = File.Create(PredictionsFilename))
            using(var bw=new BinaryWriter(f))
            {
                bw.Write(Items.Count);
                bw.Write(LastId);
                for (int i = 0; i < Items.Count; i++)
                {
                    Log?.WriteLine($"WriteItem({i})");
                    WriteItem(bw, Items[i]);
                }
            }

            for (int i = 0; i < Items.Count; i++)
            {
                if (NeedsLineSave.Contains(Items[i].Id))
                {
                    Log?.WriteLine($"WritePredictedLines({i})");
                    WritePredictedLines(Items[i]);
                }
            }
            NeedsLineSave.Clear();
        }

        private void LoadFromStorage()
        {
            Items.Clear();
            if (!File.Exists(PredictionsFilename)) 
                return;

            using (var f = File.OpenRead(PredictionsFilename))
            using (var br = new BinaryReader(f))
            {
                int itemsCount = br.ReadInt32();
                LastId = br.ReadInt32();
                for (int i = 0; i < itemsCount; i++)
                    Items.Add(ReadItem(br));
            }
        }

        private void WriteItem(BinaryWriter bw, Prediction pred)
        {
            bw.Write(pred.Id);
            bw.Write(pred.Date.ToBinary());
            pred.Image.WriteBinary(bw);
        }

        private Prediction ReadItem(BinaryReader br)
        {
            var id = br.ReadInt32();
            var date = DateTime.FromBinary(br.ReadInt64());
            var image = ImageRef.ReadBinary(br);
            return new Prediction
            {
                Id = id,
                Date = date,
                Image = image
            };
        }
        
        private void WritePredictedLines(Prediction pred)
        {
            var lines = pred.PredictedLines;
            var path = Path.Combine(PredictedLinesDir, $"{pred.Id}.dat");
            using(var f=File.Create(path))
            using(var bw=new BinaryWriter(f))
            {
                bw.Write(lines.Count);
                for (int i = 0; i < lines.Count; i++)
                    lines[i].WriteBinary(bw);                
            }
        }

        private void ReadPredictedLines(Prediction pred)
        {
            Log?.WriteLine("ReadPredictedLines");
            var lines = pred.PredictedLines = new List<PredictedLine>();
            var path = Path.Combine(PredictedLinesDir, $"{pred.Id}.dat");

            Log?.WriteLine($"path={path}");

            if (!File.Exists(path))
            {
                Log?.WriteLine($"File not found: {path}");
                return;
            }
            Log?.WriteLine($"Proceed");
            lines.Clear();

            Log?.WriteLine($"Opening file");
            using (var f = File.OpenRead(path))
            {
                Log?.WriteLine($"Opened file?");
                using (var br = new BinaryReader(f))
                {
                    Log?.WriteLine($"Before reading?");

                    int count = br.ReadInt32();
                    Log?.WriteLine($"count={count}");
                    for (int i = 0; i < count; i++)
                    {
                        Log?.WriteLine($"Line{i}");
                        lines.Add(PredictedLine.ReadBinary(br));
                    }
                }
            }

        }
        

    }
}
