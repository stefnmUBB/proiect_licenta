namespace LineSegmentationDatasetCreator
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pathsEditor1 = new LineSegmentationDatasetCreator.PathsEditor();
            this.SuspendLayout();
            // 
            // pathsEditor1
            // 
            this.pathsEditor1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pathsEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pathsEditor1.Image = null;
            this.pathsEditor1.Location = new System.Drawing.Point(0, 0);
            this.pathsEditor1.Name = "pathsEditor1";
            this.pathsEditor1.Size = new System.Drawing.Size(847, 487);
            this.pathsEditor1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 487);
            this.Controls.Add(this.pathsEditor1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private PathsEditor pathsEditor1;
    }
}

