namespace XR
{
    partial class MainWindow
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControl1 = new OpenTK.GLControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewEnvironmentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewDisplayMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewGridMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GridPropertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CameraMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CameraOrbitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CameraFpsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InspectionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(0, 24);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(784, 537);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.GlControl1_Load);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            this.glControl1.Resize += new System.EventHandler(this.GlControl1_Resize);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileOpenMenuItem,
            this.ViewMenuItem,
            this.CameraMenuItem,
            this.InspectionMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(784, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileOpenMenuItem
            // 
            this.FileOpenMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadModelToolStripMenuItem,
            this.loadAnimationToolStripMenuItem});
            this.FileOpenMenuItem.Name = "FileOpenMenuItem";
            this.FileOpenMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.FileOpenMenuItem.Size = new System.Drawing.Size(48, 20);
            this.FileOpenMenuItem.Text = "Open";
            // 
            // loadModelToolStripMenuItem
            // 
            this.loadModelToolStripMenuItem.Name = "loadModelToolStripMenuItem";
            this.loadModelToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.loadModelToolStripMenuItem.Text = "Load Model";
            this.loadModelToolStripMenuItem.Click += new System.EventHandler(this.loadModelToolStripMenuItem_Click);
            // 
            // loadAnimationToolStripMenuItem
            // 
            this.loadAnimationToolStripMenuItem.Name = "loadAnimationToolStripMenuItem";
            this.loadAnimationToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.loadAnimationToolStripMenuItem.Text = "Load Animation";
            this.loadAnimationToolStripMenuItem.Click += new System.EventHandler(this.loadAnimationToolStripMenuItem_Click);
            // 
            // ViewMenuItem
            // 
            this.ViewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewEnvironmentMenuItem,
            this.ViewDisplayMenuItem,
            this.ViewGridMenuItem});
            this.ViewMenuItem.Name = "ViewMenuItem";
            this.ViewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.ViewMenuItem.Text = "View";
            // 
            // ViewEnvironmentMenuItem
            // 
            this.ViewEnvironmentMenuItem.Name = "ViewEnvironmentMenuItem";
            this.ViewEnvironmentMenuItem.Size = new System.Drawing.Size(142, 22);
            this.ViewEnvironmentMenuItem.Text = "Environment";
            this.ViewEnvironmentMenuItem.Click += new System.EventHandler(this.ViewEnvironment_Click);
            // 
            // ViewDisplayMenuItem
            // 
            this.ViewDisplayMenuItem.Name = "ViewDisplayMenuItem";
            this.ViewDisplayMenuItem.Size = new System.Drawing.Size(142, 22);
            this.ViewDisplayMenuItem.Text = "Display";
            this.ViewDisplayMenuItem.Click += new System.EventHandler(this.ViewDisplay_Click);
            // 
            // ViewGridMenuItem
            // 
            this.ViewGridMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GridPropertiesMenuItem});
            this.ViewGridMenuItem.Name = "ViewGridMenuItem";
            this.ViewGridMenuItem.Size = new System.Drawing.Size(142, 22);
            this.ViewGridMenuItem.Text = "Grid";
            this.ViewGridMenuItem.Click += new System.EventHandler(this.ViewGrid_Click);
            // 
            // GridPropertiesMenuItem
            // 
            this.GridPropertiesMenuItem.Name = "GridPropertiesMenuItem";
            this.GridPropertiesMenuItem.Size = new System.Drawing.Size(127, 22);
            this.GridPropertiesMenuItem.Text = "Properties";
            this.GridPropertiesMenuItem.Click += new System.EventHandler(this.GridProperties_Click);
            // 
            // CameraMenuItem
            // 
            this.CameraMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CameraOrbitMenuItem,
            this.CameraFpsMenuItem});
            this.CameraMenuItem.Name = "CameraMenuItem";
            this.CameraMenuItem.Size = new System.Drawing.Size(60, 20);
            this.CameraMenuItem.Text = "Camera";
            // 
            // CameraOrbitMenuItem
            // 
            this.CameraOrbitMenuItem.Name = "CameraOrbitMenuItem";
            this.CameraOrbitMenuItem.Size = new System.Drawing.Size(101, 22);
            this.CameraOrbitMenuItem.Text = "Orbit";
            this.CameraOrbitMenuItem.Click += new System.EventHandler(this.CameraOrbit_Click);
            // 
            // CameraFpsMenuItem
            // 
            this.CameraFpsMenuItem.Name = "CameraFpsMenuItem";
            this.CameraFpsMenuItem.Size = new System.Drawing.Size(101, 22);
            this.CameraFpsMenuItem.Text = "FPS";
            this.CameraFpsMenuItem.Click += new System.EventHandler(this.CameraFPS_Click);
            // 
            // InspectionMenuItem
            // 
            this.InspectionMenuItem.Name = "InspectionMenuItem";
            this.InspectionMenuItem.Size = new System.Drawing.Size(74, 20);
            this.InspectionMenuItem.Text = "Inspection";
            this.InspectionMenuItem.Click += new System.EventHandler(this.AnimationToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IXIR";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_Closed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileOpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewDisplayMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewGridMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CameraMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CameraFpsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CameraOrbitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewEnvironmentMenuItem;
        private System.Windows.Forms.ToolStripMenuItem GridPropertiesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem InspectionMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAnimationToolStripMenuItem;
    }
}

