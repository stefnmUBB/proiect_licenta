using LillyScan.FrontendXamarin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                LoadFromStorage();
                IsLoaded = true;
            }
            return Items;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(Prediction pred)
        {
            pred.Id = LastId++;
            Items.Add(pred);
            NeedsLineSave.Add(pred.Id);
            WriteToStorage();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Remove(Prediction pred)
        {
            foreach (var item in Items.Where(_ => _.Id == pred.Id))
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
            using(var f = File.Create(PredictionsFilename))
            using(var bw=new BinaryWriter(f))
            {
                bw.Write(Items.Count);
                bw.Write(LastId);
                for (int i = 0; i < Items.Count; i++)                
                    WriteItem(bw, Items[i]);        
            }

            for (int i = 0; i < Items.Count; i++)
            {
                if (NeedsLineSave.Contains(Items[i].Id))
                    WritePredictedLines(Items[i]);
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
            var lines = pred.PredictedLines;
            var path = Path.Combine(PredictedLinesDir, $"{pred.Id}.dat");
            if(!File.Exists(path))
            {
                Debug.WriteLine($"File not found: {path}");
                return;
            }

            lines.Clear();
            using(var f= File.OpenRead(path))
            using(var br=new BinaryReader(f))
            {
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                    lines.Add(PredictedLine.ReadBinary(br));
            }

        }
        

    }
}
