using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LineSegmentationDatasetCreator
{
    public partial class PathsEditor : UserControl
    {
        public PathsEditor()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Polygons = new List<Polygon>();

            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ImageViewer, true);
        }

        private Bitmap _Image;
        public Bitmap Image
        {
            get => _Image;
            set
            {
                if (_Image != null)
                    _Image.Dispose();
                _Image = value;
                RefreshImageViewer();
            }
        }


        private void RefreshImageViewer()
        {
            if(_Image==null)
            {
                ImageViewer.Size = new Size(64, 64);
                return;
            }

            ImageViewer.Size = _Image.Size;
            Invalidate();
        }        
        

        private void ImageViewer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(Zoom * 0.01f, Zoom * 0.01f);

            if (_Image != null)
            {
                e.Graphics.DrawImageUnscaled(_Image, 0, 0);

                var rand = new Random(2023);

                var colors = typeof(KnownColor).GetFields(BindingFlags.Public | BindingFlags.Static)
                    .OrderBy(_=>rand.Next()).Take(7).ToArray();
                
                foreach (var poly in Polygons)
                {
                    var x = rand.Next() % colors.Length;
                    var color = Color.FromKnownColor((KnownColor)colors[x].GetValue(null));

                    using (var pen = new Pen(color, 2))
                    {
                        if (poly.Points.Count > 1)

                            e.Graphics.DrawPolygon(pen, poly.Points.ToArray());

                        foreach (var p in poly.Points)
                        {
                            e.Graphics.FillEllipse(Brushes.White, p.X - 2, p.Y - 2, 4, 4);
                            e.Graphics.DrawEllipse(pen, p.X - 2, p.Y - 2, 4, 4);
                        }
                    }
                                            
                }
            }
        }

        private List<Polygon> _Polygons = new List<Polygon>();

        public List<Polygon> Polygons
        {
            get => _Polygons;
            set
            {
                _Polygons = value;
                BindingPolygons = new BindingList<Polygon>(_Polygons);
                PolyList.DataSource = BindingPolygons;
            }
        }

        private BindingList<Polygon> BindingPolygons;

        private void NewButton_Click(object sender, EventArgs e)
        {
            BindingPolygons.Add(new Polygon { Name = $"Line{BindingPolygons.Count + 1}" });                               
        }

        Polygon mv_selPoly = null;
        int mv_selPoint = -1;
        int mv_x = 0, mv_y = 0;
        int mv_dx = 0, mv_dy = 0;

        Point UnZoom(Point p) => new Point(p.X * 100 / Zoom, p.Y * 100 / Zoom);        

        private void ImageViewer_MouseDown(object sender, MouseEventArgs e)
        {            

            if (RB_Insert.Checked)
            {
                if (PolyList.SelectedIndex < 0) return;
                var selectedPoly = (Polygon)PolyList.SelectedItem;
                selectedPoly.Points.Add(UnZoom(e.Location));
                ImageViewer.Invalidate();
            }
            else if(RB_Delete.Checked)
            {
                var p = UnZoom(e.Location);
                foreach(var poly in Polygons)
                {
                    for(int i=0;i<poly.Points.Count;i++)                    
                    {
                        var pt = poly.Points[i];
                        if (Math.Abs(p.X - pt.X)<=3 && Math.Abs(p.Y-pt.Y)<=3)
                        {
                            poly.Points.RemoveAt(i);
                            ImageViewer.Invalidate();
                            return;
                        }
                    }
                }
            }
            else if(RB_Move.Checked)
            {
                var p = UnZoom(e.Location);
                foreach (var poly in Polygons)
                {
                    for (int i = 0; i < poly.Points.Count; i++)
                    {
                        var pt = poly.Points[i];
                        if (Math.Abs(p.X - pt.X) <= 3 && Math.Abs(p.Y - pt.Y) <= 3)
                        {
                            mv_selPoly = poly;
                            mv_x = pt.Y;
                            mv_y = pt.Y;
                            mv_dx = p.X - pt.X;
                            mv_dy = p.Y - pt.Y;
                            mv_selPoint = i;
                            return;
                        }
                    }
                }
            }

        }

        private void ImageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (mv_selPoly == null) return;
            var p = UnZoom(e.Location);
            mv_selPoly.Points[mv_selPoint] = new Point(p.X - mv_dx, p.Y - mv_dy);
            ImageViewer.Invalidate();
        }

        string Filename = "";

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if(OFD.ShowDialog()==DialogResult.OK)
            {
                using (var bmp = new Bitmap(OFD.FileName))
                    Image = new Bitmap(bmp);
                Filename = OFD.FileName;
                Polygons.Clear();

                var polyFile = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".seg.bin";
                if(File.Exists(polyFile))
                {
                    var bytes = File.ReadAllBytes(polyFile);
                    bytes = bytes.Take((bytes.Length / 4) * 4).ToArray();
                    var numFloats = bytes.Length / 4;

                    using(var ms = new MemoryStream(bytes))
                    {
                        using(var br=new BinaryReader(ms))
                        {
                            var lst = new List<Point>();
                            int i = 0;
                            while(i<numFloats)
                            {                                
                                var x = br.ReadSingle(); i++;
                                if(x<0)
                                {
                                    if (lst.Count == 0) continue;
                                    BindingPolygons.Add(new Polygon { Name = $"Poly{Polygons.Count + 1}", Points = lst });
                                    lst = new List<Point>();
                                    continue;
                                }

                                if (i == numFloats) break;                                
                                var y = br.ReadSingle(); i++;
                                lst.Add(new Point((int)(x * Image.Width), (int)(y * Image.Height)));
                            }                            
                        }
                    }

                    ImageViewer.Invalidate();
                    Polygons = Polygons;

                    FindForm().Text = $"{Path.GetFileName(Filename)} ({Image.Width}x{Image.Height})";
                }                
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (var f = File.OpenWrite(Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".seg.bin"))
            using (var bw = new BinaryWriter(f))
            {
                foreach (var poly in Polygons)
                {
                    foreach (var pt in poly.Points)
                    {
                        bw.Write((float)(1.0 * pt.X / Image.Width));
                        bw.Write((float)(1.0 * pt.Y / Image.Height));
                    }
                    bw.Write((float)-1);
                    bw.Write((float)-1);
                }
                bw.Write((float)-1);
            }
        }

        private int Zoom = 100;

        private void Zoom50Button_Click(object sender, EventArgs e)
        {
            Zoom = 50;
            ImageViewer.Invalidate();
        }

        private void Zoom75Button_Click(object sender, EventArgs e)
        {
            Zoom = 75;
            ImageViewer.Invalidate();
        }

        private void Zoom100Button_Click(object sender, EventArgs e)
        {
            Zoom = 100;
            ImageViewer.Invalidate();
        }

        private void Zoom25_Click(object sender, EventArgs e)
        {
            Zoom = 25;
            ImageViewer.Invalidate();
        }

        private void ImageViewer_MouseUp(object sender, MouseEventArgs e)
        {
            mv_selPoly = null;
        }

        private void ImageViewer_MouseLeave(object sender, EventArgs e)
        {
            mv_selPoly = null;
        }
    }
}
