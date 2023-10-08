using System.Windows.Forms;

namespace Helpers.CurveDetectorDataSetGenerator
{
    public partial class Form1 : Form
    {                
        public Form1()
        {
            InitializeComponent();
            typeof(Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(Canvas, true);  
            


        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            
        }
    }
}
