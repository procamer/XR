namespace XR
{
    partial class Inspector
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Inspector));
            this.label3 = new System.Windows.Forms.Label();
            this.listBoxAnimations = new System.Windows.Forms.ListBox();
            this.panelAnimTools = new System.Windows.Forms.Panel();
            this.btnLoadAnim = new System.Windows.Forms.Button();
            this.hScrollCursor = new System.Windows.Forms.HScrollBar();
            this.label1 = new System.Windows.Forms.Label();
            this.timeSlideControl = new XR.TimeSlideControl();
            this.labelGotoError = new System.Windows.Forms.Label();
            this.checkBoxLoop = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonFaster = new System.Windows.Forms.Button();
            this.buttonSlower = new System.Windows.Forms.Button();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.imageListTree = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageTree = new System.Windows.Forms.TabPage();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.tabPageAnimation = new System.Windows.Forms.TabPage();
            this.panelAnimTools.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageTree.SuspendLayout();
            this.tabPageAnimation.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Available Animations:";
            // 
            // listBoxAnimations
            // 
            this.listBoxAnimations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxAnimations.FormattingEnabled = true;
            this.listBoxAnimations.Location = new System.Drawing.Point(8, 19);
            this.listBoxAnimations.Name = "listBoxAnimations";
            this.listBoxAnimations.ScrollAlwaysVisible = true;
            this.listBoxAnimations.Size = new System.Drawing.Size(357, 186);
            this.listBoxAnimations.TabIndex = 12;
            this.listBoxAnimations.SelectedIndexChanged += new System.EventHandler(this.listBoxAnimations_SelectedIndexChanged);
            this.listBoxAnimations.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxAnimations_MouseDoubleClick);
            // 
            // panelAnimTools
            // 
            this.panelAnimTools.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAnimTools.Controls.Add(this.btnLoadAnim);
            this.panelAnimTools.Controls.Add(this.hScrollCursor);
            this.panelAnimTools.Controls.Add(this.label1);
            this.panelAnimTools.Controls.Add(this.timeSlideControl);
            this.panelAnimTools.Controls.Add(this.labelGotoError);
            this.panelAnimTools.Controls.Add(this.checkBoxLoop);
            this.panelAnimTools.Controls.Add(this.panel1);
            this.panelAnimTools.Location = new System.Drawing.Point(8, 211);
            this.panelAnimTools.Name = "panelAnimTools";
            this.panelAnimTools.Size = new System.Drawing.Size(357, 313);
            this.panelAnimTools.TabIndex = 19;
            // 
            // btnLoadAnim
            // 
            this.btnLoadAnim.Location = new System.Drawing.Point(178, 124);
            this.btnLoadAnim.Name = "btnLoadAnim";
            this.btnLoadAnim.Size = new System.Drawing.Size(148, 24);
            this.btnLoadAnim.TabIndex = 24;
            this.btnLoadAnim.Text = "Load Mixamo Animation";
            this.btnLoadAnim.UseVisualStyleBackColor = true;
            this.btnLoadAnim.Click += new System.EventHandler(this.btnLoadAnim_Click);
            // 
            // hScrollCursor
            // 
            this.hScrollCursor.LargeChange = 1;
            this.hScrollCursor.Location = new System.Drawing.Point(158, 91);
            this.hScrollCursor.Name = "hScrollCursor";
            this.hScrollCursor.Size = new System.Drawing.Size(146, 25);
            this.hScrollCursor.TabIndex = 23;
            this.hScrollCursor.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollCursor_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-1, 145);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "label1";
            // 
            // timeSlideControl
            // 
            this.timeSlideControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timeSlideControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.timeSlideControl.Location = new System.Drawing.Point(0, 0);
            this.timeSlideControl.Name = "timeSlideControl";
            this.timeSlideControl.Position = 0D;
            this.timeSlideControl.RangeMax = 0D;
            this.timeSlideControl.RangeMin = 0D;
            this.timeSlideControl.Size = new System.Drawing.Size(357, 80);
            this.timeSlideControl.TabIndex = 22;
            // 
            // labelGotoError
            // 
            this.labelGotoError.AutoSize = true;
            this.labelGotoError.Location = new System.Drawing.Point(57, 124);
            this.labelGotoError.Name = "labelGotoError";
            this.labelGotoError.Size = new System.Drawing.Size(0, 13);
            this.labelGotoError.TabIndex = 21;
            // 
            // checkBoxLoop
            // 
            this.checkBoxLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxLoop.AutoSize = true;
            this.checkBoxLoop.Location = new System.Drawing.Point(307, 91);
            this.checkBoxLoop.Name = "checkBoxLoop";
            this.checkBoxLoop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxLoop.Size = new System.Drawing.Size(50, 17);
            this.checkBoxLoop.TabIndex = 12;
            this.checkBoxLoop.Text = "Loop";
            this.checkBoxLoop.UseVisualStyleBackColor = true;
            this.checkBoxLoop.CheckedChanged += new System.EventHandler(this.checkBoxLoop_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonFaster);
            this.panel1.Controls.Add(this.buttonSlower);
            this.panel1.Controls.Add(this.buttonPlay);
            this.panel1.Location = new System.Drawing.Point(-1, 88);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(156, 50);
            this.panel1.TabIndex = 19;
            // 
            // buttonFaster
            // 
            this.buttonFaster.FlatAppearance.BorderSize = 2;
            this.buttonFaster.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.buttonFaster.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.buttonFaster.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonFaster.Location = new System.Drawing.Point(101, 3);
            this.buttonFaster.Name = "buttonFaster";
            this.buttonFaster.Size = new System.Drawing.Size(50, 40);
            this.buttonFaster.TabIndex = 14;
            this.buttonFaster.Text = "Faster";
            this.buttonFaster.UseVisualStyleBackColor = true;
            this.buttonFaster.Click += new System.EventHandler(this.buttonFaster_Click);
            // 
            // buttonSlower
            // 
            this.buttonSlower.FlatAppearance.BorderSize = 2;
            this.buttonSlower.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.buttonSlower.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.buttonSlower.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonSlower.Location = new System.Drawing.Point(3, 3);
            this.buttonSlower.Name = "buttonSlower";
            this.buttonSlower.Size = new System.Drawing.Size(50, 40);
            this.buttonSlower.TabIndex = 13;
            this.buttonSlower.Text = "Slower";
            this.buttonSlower.UseVisualStyleBackColor = true;
            this.buttonSlower.Click += new System.EventHandler(this.buttonSlower_Click);
            // 
            // buttonPlay
            // 
            this.buttonPlay.FlatAppearance.BorderSize = 2;
            this.buttonPlay.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.buttonPlay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.WhiteSmoke;
            this.buttonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonPlay.Location = new System.Drawing.Point(57, 3);
            this.buttonPlay.MaximumSize = new System.Drawing.Size(100, 100);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(40, 40);
            this.buttonPlay.TabIndex = 11;
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // imageListTree
            // 
            this.imageListTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTree.ImageStream")));
            this.imageListTree.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTree.Images.SetKeyName(0, "PlayAnim.png");
            this.imageListTree.Images.SetKeyName(1, "StopAnim.png");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageAnimation);
            this.tabControl1.Controls.Add(this.tabPageTree);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(384, 561);
            this.tabControl1.TabIndex = 20;
            // 
            // tabPageTree
            // 
            this.tabPageTree.Controls.Add(this.treeView1);
            this.tabPageTree.Location = new System.Drawing.Point(4, 22);
            this.tabPageTree.Name = "tabPageTree";
            this.tabPageTree.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTree.Size = new System.Drawing.Size(376, 535);
            this.tabPageTree.TabIndex = 0;
            this.tabPageTree.Text = "Tree";
            this.tabPageTree.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(3, 3);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(370, 529);
            this.treeView1.TabIndex = 2;
            // 
            // tabPageAnimation
            // 
            this.tabPageAnimation.Controls.Add(this.label3);
            this.tabPageAnimation.Controls.Add(this.panelAnimTools);
            this.tabPageAnimation.Controls.Add(this.listBoxAnimations);
            this.tabPageAnimation.Location = new System.Drawing.Point(4, 22);
            this.tabPageAnimation.Name = "tabPageAnimation";
            this.tabPageAnimation.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAnimation.Size = new System.Drawing.Size(376, 535);
            this.tabPageAnimation.TabIndex = 1;
            this.tabPageAnimation.Text = "Animations";
            this.tabPageAnimation.UseVisualStyleBackColor = true;
            // 
            // Inspector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 561);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 600);
            this.Name = "Inspector";
            this.ShowInTaskbar = false;
            this.Text = "Inspector";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Animation_Load);
            this.panelAnimTools.ResumeLayout(false);
            this.panelAnimTools.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageTree.ResumeLayout(false);
            this.tabPageAnimation.ResumeLayout(false);
            this.tabPageAnimation.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox listBoxAnimations;
        private System.Windows.Forms.Panel panelAnimTools;
        private System.Windows.Forms.Label labelGotoError;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonFaster;
        private System.Windows.Forms.Button buttonSlower;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.CheckBox checkBoxLoop;
        private TimeSlideControl timeSlideControl;
        private System.Windows.Forms.ImageList imageListTree;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageTree;
        private System.Windows.Forms.TabPage tabPageAnimation;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.HScrollBar hScrollCursor;
        private System.Windows.Forms.Button btnLoadAnim;
    }
}