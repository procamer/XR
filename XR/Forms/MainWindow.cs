using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace XR
{
    public partial class MainWindow : Form
    {
        public static List<PointLight> pointLights = new List<PointLight>();
        public static  Model activeScene;
        private FPSTracker fpsTracker;
        private double accTime;

        private Grid2D grid2D;
        private Environment environment;
        public Label labelDisplay;
        private Inspector inspector;
        private Animator animator;

        private bool glControlLoaded = false;
        private ICameraController ActiveCamera;

        public static Size RenderSize { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            // ondalık ayırıcı için gerekli. float için nokta virgül olayı.
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            glControl1.MouseWheel += OnMouseMove;
            KeyPreview = true;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            environment = new Environment();
            labelDisplay = new Label();
            grid2D = new Grid2D();
            fpsTracker = new FPSTracker();

            // Light1
            pointLights.Add(new PointLight()
            {
                position = new Vector3(10.0f, 5.0f, 10.0f),
                ambient = new Vector3(20.0f),
                diffuse = new Vector3(40.0f),
                specular = new Vector3(250.0f),
                linear = 10.0f,
                quadratic = 0.001f
            });

            // Light2
            pointLights.Add(pointLights[0]);
            pointLights[1].position = new Vector3(-10.0f, 5.0f, 10.0f);

            // Light3
            pointLights.Add(pointLights[0]);
            pointLights[2].position = new Vector3(-10.0f, 5.0f, -10.0f);

            // Light4
            pointLights.Add(pointLights[0]);
            pointLights[3].position = new Vector3(10.0f, 5.0f, -10.0f);

            ViewDisplayMenuItem.Checked = Settings.Core.Default.ViewDisplay;
            ViewGridMenuItem.Checked = Settings.Core.Default.ViewGrid;
            ViewEnvironmentMenuItem.Checked = Settings.Core.Default.EnvironmentMap;
            CameraOrbit_Click(null, null);
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Core.Default.Save();
            Settings.Graphics.Default.Save();
            Settings.Properties.Default.Save();
        }

        private void MainWindow_Closed(object sender, FormClosedEventArgs e)
        {
            if (activeScene != null)
            {
                activeScene.Dispose();
                activeScene = null;
            }
            fpsTracker.Dispose();
            labelDisplay.Dispose();
            if (inspector != null) inspector.Close();
            //if (inspection != null) inspection.Close();
        }

        private void GlControl1_Load(object sender, EventArgs e)
        {
            glControlLoaded = true;
            Application.Idle += ApplicationIdle;
        }

        private void GlControl1_Resize(object sender, EventArgs e)
        {
            RenderSize = glControl1.ClientSize;
            GL.Viewport(RenderSize);
            if (labelDisplay != null) labelDisplay.Resize();
        }

        private void ViewEnvironment_Click(object sender, EventArgs e)
        {
            if (activeScene != null)
            {
                ViewEnvironmentMenuItem.Checked = !ViewEnvironmentMenuItem.Checked;
                Settings.Core.Default.EnvironmentMap = ViewEnvironmentMenuItem.Checked;
            }
        }

        private void ViewDisplay_Click(object sender, EventArgs e)
        {
            if (activeScene != null)
            {
                ViewDisplayMenuItem.Checked = !ViewDisplayMenuItem.Checked;
                Settings.Core.Default.ViewDisplay = ViewDisplayMenuItem.Checked;
            }
        }

        private void ViewGrid_Click(object sender, EventArgs e)
        {
            if (activeScene != null && ActiveCamera != null)
            {
                ViewGridMenuItem.Checked = !ViewGridMenuItem.Checked;
                Settings.Core.Default.ViewGrid = ViewGridMenuItem.Checked;
            }
        }

        private void GridProperties_Click(object sender, EventArgs e)
        {
            PropertiesGrid preferencesGrid = new PropertiesGrid();
            preferencesGrid.Show();
        }

        private void CameraOrbit_Click(object sender, EventArgs e)
        {
            CameraOrbitMenuItem.Checked = true;
            CameraFpsMenuItem.Checked = false;
            ActiveCamera = new CameraOrbit(Vector3.UnitZ * 5f);
        }

        private void CameraFPS_Click(object sender, EventArgs e)
        {
            CameraOrbitMenuItem.Checked = false;
            CameraFpsMenuItem.Checked = true;
            ActiveCamera = new CameraFPS(Vector3.UnitZ * 5f)
            {
                Yaw = -90f,
            };
        }

        private void AnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activeScene != null && activeScene.Animator != null)
            {
                inspector = new Inspector(activeScene);
                inspector.Show();
            }
        }

        private void ApplicationIdle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                if (activeScene != null && ActiveCamera != null)
                {
                    FrameUpdate();
                    FrameRender();
                }
            }
        }

        public void GetDisplay()
        {
            double _displayFps;

            // only update every 1/3rd of a second
            accTime += fpsTracker.LastFrameDelta;
            if (accTime < 0.3333 && !labelDisplay.WantRedraw)
            {
                if (accTime >= 0.3333)
                {
                    labelDisplay.WantRedrawNextFrame = true;
                }
                return;
            }

            if (accTime >= 0.3333)
            {
                _displayFps = fpsTracker.LastFps;
                accTime = 0.0;
                string distanceFOV = (ActiveCamera is CameraOrbit)
                    ? " Distance: " + ActiveCamera.Distance.ToString("0.0")
                    : " FOV: " + ActiveCamera.Fov.ToString("0.0") + " deg";

                labelDisplay.Text = string.Format(" FPS: {0}\n{1}\n Pitch: {2} deg\n Yaw: {3} deg\n",
                    _displayFps.ToString("0.0"),
                    distanceFOV,
                    ActiveCamera.Pitch.ToString("0.0"),
                    ActiveCamera.Yaw.ToString("0.0"));
            }
        }

        private void FrameUpdate()
        {
            fpsTracker.Update();

            var delta = fpsTracker.LastFrameDelta;

            if (GridProperties.Dirty)
            {
                grid2D = new Grid2D();
                GridProperties.Dirty = false;
            }

            activeScene.Update(delta);
            if (ActiveCamera != null) ActiveCamera.MovementKey((float)fpsTracker.LastFrameDelta);
            if (ViewDisplayMenuItem.Checked) GetDisplay();
        }

        private void FrameRender()
        {
            if (glControlLoaded && activeScene != null && ActiveCamera != null)
            {
                GL.ClearColor(0.6f, 0.7f, 0.8f, 1.0f);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

                Matrix4 view = ActiveCamera.GetViewMatrix();
                Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(ActiveCamera.Fov),
                    (RenderSize.Width / (float)RenderSize.Height),
                    0.01f, 1000f);

                activeScene.Renderer.Render(view, perspective);

                if (ViewEnvironmentMenuItem.Checked) environment.Render(view, perspective);

                if (ViewGridMenuItem.Checked) grid2D.Render(view, perspective);

                if (ViewDisplayMenuItem.Checked) labelDisplay.Render();

                glControl1.SwapBuffers();
            }
        }

        #region Camera

        private int _previousMousePosX = -1;
        private int _previousMousePosY = -1;

        private bool _mouseLeftDown;
        private bool _mouseMiddleDown;
        //private bool _mouseRightDown;

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            _previousMousePosX = e.X;
            _previousMousePosY = e.Y;

            if (e.Button == MouseButtons.Left)
            {
                _mouseLeftDown = true;
                return;
            }

            if (e.Button == MouseButtons.Middle)
            {
                _mouseMiddleDown = true;
                return;
            }

            //if (e.Button == MouseButtons.Right)
            //{
            //    _mouseRightDown = true;
            //    return;
            //}

        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseLeftDown = false;
                return;
            }

            if (e.Button == MouseButtons.Middle)
            {
                _mouseMiddleDown = false;
                return;
            }

            //if (e.Button == MouseButtons.Right)
            //{
            //    _mouseRightDown = false;
            //    return;
            //}

        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ActiveCamera == null)
                return;

            if (e.Delta != 0)
                ActiveCamera.Scroll(e.Delta);

            if (_mouseMiddleDown)
                ActiveCamera.Pan(e.X - _previousMousePosX, e.Y - _previousMousePosY);

            if (_mouseLeftDown)
                ActiveCamera.MouseMove(e.X - _previousMousePosX, e.Y - _previousMousePosY);

            _previousMousePosX = e.X;
            _previousMousePosY = e.Y;
        }

        #endregion

        //private void FileOpen_Click(object sender, EventArgs e)
        //{
        //}

        private void OpenModelFile(string filename)
        {
            try
            {
                if (activeScene != null) activeScene.Dispose();
                if (inspector != null) inspector.Close();
                activeScene = new Model(filename);
                animator = activeScene.Animator;
            }
            catch (Exception)
            {
            }

        }

        private void OpenAnimationFile(string filename)
        {
            try
            {
                //if (activeScene != null) activeScene.Dispose();
                inspector?.Close();
                animator = new Animator(filename);
                activeScene.Animator = animator;
            }
            catch (Exception)
            {
            }

        }

        private void loadModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Filter = Utility.GetSupportedImportFormat2(),
                InitialDirectory = Application.StartupPath + "\\Resources\\Model"                
            };

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                OpenModelFile(openFile.FileName);
                Text = Path.GetFileName(openFile.FileName);
            }
        }

        private void loadAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Filter = Utility.GetSupportedImportFormat2()
            };

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                OpenAnimationFile(openFile.FileName);
                Text = Path.GetFileName(openFile.FileName);
            }
        }
    }
}
