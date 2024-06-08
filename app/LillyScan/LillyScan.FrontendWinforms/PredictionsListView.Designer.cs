namespace LillyScan.FrontendWinforms
{
    partial class PredictionsListView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Scrollbar = new System.Windows.Forms.VScrollBar();
            this.SuspendLayout();
            // 
            // Scrollbar
            // 
            this.Scrollbar.Dock = System.Windows.Forms.DockStyle.Right;
            this.Scrollbar.Location = new System.Drawing.Point(447, 0);
            this.Scrollbar.Name = "Scrollbar";
            this.Scrollbar.Size = new System.Drawing.Size(17, 179);
            this.Scrollbar.TabIndex = 0;
            this.Scrollbar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Scrollbar_Scroll);
            // 
            // PredictionsListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Scrollbar);
            this.Name = "PredictionsListView";
            this.Size = new System.Drawing.Size(464, 179);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar Scrollbar;
    }
}
