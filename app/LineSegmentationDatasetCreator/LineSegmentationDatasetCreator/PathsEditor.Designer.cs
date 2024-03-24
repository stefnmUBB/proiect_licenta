namespace LineSegmentationDatasetCreator
{
    partial class PathsEditor
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ImageViewer = new System.Windows.Forms.Panel();
            this.SaveButton = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RB_Delete = new System.Windows.Forms.RadioButton();
            this.RB_Move = new System.Windows.Forms.RadioButton();
            this.RB_Insert = new System.Windows.Forms.RadioButton();
            this.NewButton = new System.Windows.Forms.Button();
            this.PolyList = new System.Windows.Forms.ListBox();
            this.OFD = new System.Windows.Forms.OpenFileDialog();
            this.Zoom50Button = new System.Windows.Forms.Button();
            this.Zoom100Button = new System.Windows.Forms.Button();
            this.Zoom75Button = new System.Windows.Forms.Button();
            this.Zoom25 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.ImageViewer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Zoom25);
            this.splitContainer1.Panel2.Controls.Add(this.Zoom75Button);
            this.splitContainer1.Panel2.Controls.Add(this.Zoom100Button);
            this.splitContainer1.Panel2.Controls.Add(this.Zoom50Button);
            this.splitContainer1.Panel2.Controls.Add(this.SaveButton);
            this.splitContainer1.Panel2.Controls.Add(this.LoadButton);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.NewButton);
            this.splitContainer1.Panel2.Controls.Add(this.PolyList);
            this.splitContainer1.Size = new System.Drawing.Size(736, 489);
            this.splitContainer1.SplitterDistance = 530;
            this.splitContainer1.TabIndex = 1;
            // 
            // ImageViewer
            // 
            this.ImageViewer.Location = new System.Drawing.Point(0, 0);
            this.ImageViewer.Name = "ImageViewer";
            this.ImageViewer.Size = new System.Drawing.Size(64, 64);
            this.ImageViewer.TabIndex = 0;
            this.ImageViewer.Paint += new System.Windows.Forms.PaintEventHandler(this.ImageViewer_Paint);
            this.ImageViewer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ImageViewer_MouseDown);
            this.ImageViewer.MouseLeave += new System.EventHandler(this.ImageViewer_MouseLeave);
            this.ImageViewer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ImageViewer_MouseMove);
            this.ImageViewer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ImageViewer_MouseUp);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(90, 317);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 4;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.Location = new System.Drawing.Point(9, 317);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(75, 23);
            this.LoadButton.TabIndex = 3;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RB_Delete);
            this.groupBox1.Controls.Add(this.RB_Move);
            this.groupBox1.Controls.Add(this.RB_Insert);
            this.groupBox1.Location = new System.Drawing.Point(3, 211);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(196, 100);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mode";
            // 
            // RB_Delete
            // 
            this.RB_Delete.AutoSize = true;
            this.RB_Delete.Location = new System.Drawing.Point(6, 65);
            this.RB_Delete.Name = "RB_Delete";
            this.RB_Delete.Size = new System.Drawing.Size(56, 17);
            this.RB_Delete.TabIndex = 2;
            this.RB_Delete.Text = "Delete";
            this.RB_Delete.UseVisualStyleBackColor = true;
            // 
            // RB_Move
            // 
            this.RB_Move.AutoSize = true;
            this.RB_Move.Location = new System.Drawing.Point(6, 42);
            this.RB_Move.Name = "RB_Move";
            this.RB_Move.Size = new System.Drawing.Size(52, 17);
            this.RB_Move.TabIndex = 1;
            this.RB_Move.Text = "Move";
            this.RB_Move.UseVisualStyleBackColor = true;
            // 
            // RB_Insert
            // 
            this.RB_Insert.AutoSize = true;
            this.RB_Insert.Checked = true;
            this.RB_Insert.Location = new System.Drawing.Point(6, 19);
            this.RB_Insert.Name = "RB_Insert";
            this.RB_Insert.Size = new System.Drawing.Size(51, 17);
            this.RB_Insert.TabIndex = 0;
            this.RB_Insert.TabStop = true;
            this.RB_Insert.Text = "Insert";
            this.RB_Insert.UseVisualStyleBackColor = true;
            // 
            // NewButton
            // 
            this.NewButton.Location = new System.Drawing.Point(3, 3);
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(75, 23);
            this.NewButton.TabIndex = 1;
            this.NewButton.Text = "New";
            this.NewButton.UseVisualStyleBackColor = true;
            this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
            // 
            // PolyList
            // 
            this.PolyList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PolyList.FormattingEnabled = true;
            this.PolyList.Location = new System.Drawing.Point(3, 32);
            this.PolyList.Name = "PolyList";
            this.PolyList.Size = new System.Drawing.Size(196, 173);
            this.PolyList.TabIndex = 0;
            // 
            // OFD
            // 
            this.OFD.Filter = "*.png|*.png|*.jpg|*.jpg|*.jpeg|*.jpeg";
            // 
            // Zoom50Button
            // 
            this.Zoom50Button.Location = new System.Drawing.Point(9, 376);
            this.Zoom50Button.Name = "Zoom50Button";
            this.Zoom50Button.Size = new System.Drawing.Size(51, 23);
            this.Zoom50Button.TabIndex = 5;
            this.Zoom50Button.Text = "50%";
            this.Zoom50Button.UseVisualStyleBackColor = true;
            this.Zoom50Button.Click += new System.EventHandler(this.Zoom50Button_Click);
            // 
            // Zoom100Button
            // 
            this.Zoom100Button.Location = new System.Drawing.Point(123, 376);
            this.Zoom100Button.Name = "Zoom100Button";
            this.Zoom100Button.Size = new System.Drawing.Size(56, 23);
            this.Zoom100Button.TabIndex = 6;
            this.Zoom100Button.Text = "100%";
            this.Zoom100Button.UseVisualStyleBackColor = true;
            this.Zoom100Button.Click += new System.EventHandler(this.Zoom100Button_Click);
            // 
            // Zoom75Button
            // 
            this.Zoom75Button.Location = new System.Drawing.Point(66, 376);
            this.Zoom75Button.Name = "Zoom75Button";
            this.Zoom75Button.Size = new System.Drawing.Size(51, 23);
            this.Zoom75Button.TabIndex = 7;
            this.Zoom75Button.Text = "75%";
            this.Zoom75Button.UseVisualStyleBackColor = true;
            this.Zoom75Button.Click += new System.EventHandler(this.Zoom75Button_Click);
            // 
            // Zoom25
            // 
            this.Zoom25.Location = new System.Drawing.Point(9, 347);
            this.Zoom25.Name = "Zoom25";
            this.Zoom25.Size = new System.Drawing.Size(51, 23);
            this.Zoom25.TabIndex = 8;
            this.Zoom25.Text = "25%";
            this.Zoom25.UseVisualStyleBackColor = true;
            this.Zoom25.Click += new System.EventHandler(this.Zoom25_Click);
            // 
            // PathsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "PathsEditor";
            this.Size = new System.Drawing.Size(736, 489);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel ImageViewer;
        private System.Windows.Forms.Button NewButton;
        private System.Windows.Forms.ListBox PolyList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton RB_Delete;
        private System.Windows.Forms.RadioButton RB_Move;
        private System.Windows.Forms.RadioButton RB_Insert;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.OpenFileDialog OFD;
        private System.Windows.Forms.Button Zoom75Button;
        private System.Windows.Forms.Button Zoom100Button;
        private System.Windows.Forms.Button Zoom50Button;
        private System.Windows.Forms.Button Zoom25;
    }
}
