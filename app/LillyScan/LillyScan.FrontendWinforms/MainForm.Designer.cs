namespace LillyScan.FrontendWinforms
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
            this.Body = new System.Windows.Forms.TabControl();
            this.MainPage = new System.Windows.Forms.TabPage();
            this.MainLoadImageControlmageControl2 = new LillyScan.FrontendWinforms.LoadImageControl();
            this.LineRecognitionPage = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.LineRecogOutBox = new System.Windows.Forms.TextBox();
            this.LineRecogLogsView = new LillyScan.FrontendWinforms.LogsView();
            this.LineRecogLoadImageControl = new LillyScan.FrontendWinforms.LoadImageControl();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.MainLogsView = new LillyScan.FrontendWinforms.LogsView();
            this.PredictionsListView = new LillyScan.FrontendWinforms.PredictionsListView();
            this.Body.SuspendLayout();
            this.MainPage.SuspendLayout();
            this.LineRecognitionPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // Body
            // 
            this.Body.Controls.Add(this.MainPage);
            this.Body.Controls.Add(this.LineRecognitionPage);
            this.Body.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Body.Location = new System.Drawing.Point(0, 0);
            this.Body.Name = "Body";
            this.Body.SelectedIndex = 0;
            this.Body.Size = new System.Drawing.Size(738, 360);
            this.Body.TabIndex = 0;
            // 
            // MainPage
            // 
            this.MainPage.Controls.Add(this.splitContainer2);
            this.MainPage.Controls.Add(this.MainLoadImageControlmageControl2);
            this.MainPage.Location = new System.Drawing.Point(4, 22);
            this.MainPage.Name = "MainPage";
            this.MainPage.Padding = new System.Windows.Forms.Padding(3);
            this.MainPage.Size = new System.Drawing.Size(730, 334);
            this.MainPage.TabIndex = 0;
            this.MainPage.Text = "Main Page";
            this.MainPage.UseVisualStyleBackColor = true;
            // 
            // MainLoadImageControlmageControl2
            // 
            this.MainLoadImageControlmageControl2.AllowDrop = true;
            this.MainLoadImageControlmageControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MainLoadImageControlmageControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainLoadImageControlmageControl2.Location = new System.Drawing.Point(3, 3);
            this.MainLoadImageControlmageControl2.Name = "MainLoadImageControlmageControl2";
            this.MainLoadImageControlmageControl2.Size = new System.Drawing.Size(724, 125);
            this.MainLoadImageControlmageControl2.TabIndex = 1;
            this.MainLoadImageControlmageControl2.InputChanged += new LillyScan.FrontendWinforms.LoadImageControl.OnInputChanged(this.MainLoadImageControlmageControl2_InputChanged);
            // 
            // LineRecognitionPage
            // 
            this.LineRecognitionPage.Controls.Add(this.splitContainer1);
            this.LineRecognitionPage.Controls.Add(this.LineRecogLoadImageControl);
            this.LineRecognitionPage.Location = new System.Drawing.Point(4, 22);
            this.LineRecognitionPage.Name = "LineRecognitionPage";
            this.LineRecognitionPage.Padding = new System.Windows.Forms.Padding(3);
            this.LineRecognitionPage.Size = new System.Drawing.Size(730, 334);
            this.LineRecognitionPage.TabIndex = 1;
            this.LineRecognitionPage.Text = "Line Recognition Only";
            this.LineRecognitionPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 128);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.LineRecogOutBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LineRecogLogsView);
            this.splitContainer1.Size = new System.Drawing.Size(724, 203);
            this.splitContainer1.SplitterDistance = 241;
            this.splitContainer1.TabIndex = 1;
            // 
            // LineRecogOutBox
            // 
            this.LineRecogOutBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LineRecogOutBox.Location = new System.Drawing.Point(0, 0);
            this.LineRecogOutBox.Multiline = true;
            this.LineRecogOutBox.Name = "LineRecogOutBox";
            this.LineRecogOutBox.ReadOnly = true;
            this.LineRecogOutBox.Size = new System.Drawing.Size(241, 203);
            this.LineRecogOutBox.TabIndex = 0;
            // 
            // LineRecogLogsView
            // 
            this.LineRecogLogsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LineRecogLogsView.Location = new System.Drawing.Point(0, 0);
            this.LineRecogLogsView.Name = "LineRecogLogsView";
            this.LineRecogLogsView.Size = new System.Drawing.Size(479, 203);
            this.LineRecogLogsView.TabIndex = 0;
            // 
            // LineRecogLoadImageControl
            // 
            this.LineRecogLoadImageControl.AllowDrop = true;
            this.LineRecogLoadImageControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LineRecogLoadImageControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.LineRecogLoadImageControl.Location = new System.Drawing.Point(3, 3);
            this.LineRecogLoadImageControl.Name = "LineRecogLoadImageControl";
            this.LineRecogLoadImageControl.Size = new System.Drawing.Size(724, 125);
            this.LineRecogLoadImageControl.TabIndex = 0;
            this.LineRecogLoadImageControl.InputChanged += new LillyScan.FrontendWinforms.LoadImageControl.OnInputChanged(this.LineRecogLoadImageControl_InputChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 128);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.PredictionsListView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.MainLogsView);
            this.splitContainer2.Size = new System.Drawing.Size(724, 203);
            this.splitContainer2.SplitterDistance = 467;
            this.splitContainer2.TabIndex = 2;
            // 
            // MainLogsView
            // 
            this.MainLogsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLogsView.Location = new System.Drawing.Point(0, 0);
            this.MainLogsView.Name = "MainLogsView";
            this.MainLogsView.Size = new System.Drawing.Size(253, 203);
            this.MainLogsView.TabIndex = 0;
            // 
            // PredictionsListView
            // 
            this.PredictionsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PredictionsListView.Location = new System.Drawing.Point(0, 0);
            this.PredictionsListView.Name = "PredictionsListView";
            this.PredictionsListView.Size = new System.Drawing.Size(467, 203);
            this.PredictionsListView.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 360);
            this.Controls.Add(this.Body);
            this.Name = "MainForm";
            this.Text = "LillyScan Desktop";
            this.Body.ResumeLayout(false);
            this.MainPage.ResumeLayout(false);
            this.LineRecognitionPage.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Body;
        private System.Windows.Forms.TabPage MainPage;
        private System.Windows.Forms.TabPage LineRecognitionPage;
        private LoadImageControl LineRecogLoadImageControl;
        private LoadImageControl MainLoadImageControlmageControl2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private LogsView LineRecogLogsView;
        private System.Windows.Forms.TextBox LineRecogOutBox;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private LogsView MainLogsView;
        private PredictionsListView PredictionsListView;
    }
}

