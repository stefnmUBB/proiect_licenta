using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LineSegmentationDatasetCreator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            pathsEditor1.Image = new Bitmap(@"D:\Users\Stefan\Datasets\newscr\C1\50.png");            
        }
    }
}
