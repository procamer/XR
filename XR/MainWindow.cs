using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace XR
{
    public partial class MainWindow : Form
    {
        private Model activeScene;
        private Grid2D grid2D;
        private Environment environment;
        public Label labelFPS;
        private FPSTracker fpsTracker;
        
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
            labelFPS = new Label();
            grid2D = new Grid2D();
            fpsTracker = new FPSTracker();

            ViewFPSMenuItem.Checked = Settings.Core.Default.ViewFPS;
            ViewGridMenuItem.Checked = Settings.Core.Default.ViewGrid;
            ViewEnvironmentMenuItem.Checked = Settings.Core.Default.EnvironmentMap;

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
            labelFPS.Dispose();
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
            if(labelFPS != null) labelFPS.Resize();
        }

        private void FileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Filter = Utility.GetSupportedImportFormat2()
            };

            if (openFile.ShowDialog() == DialogResult.OK)
                OpenFile(openFile.FileName);
        }

        private void ViewEnvironment_Click(object sender, EventArgs e)
        {
            if (activeScene != null)
            {
                ViewEnvironmentMenuItem.Checked = !ViewEnvironmentMenuItem.Checked;
                Settings.Core.Default.EnvironmentMap = ViewEnvironmentMenuItem.Checked;
            }
        }

        private void ViewFPS_Click(object sender, EventArgs e)
        {
            if (activeScene != null)
            {
                ViewFPSMenuItem.Checked = !ViewFPSMenuItem.Checked;
                Settings.Core.Default.ViewFPS = ViewFPSMenuItem.Checked;
            }
        }

        private void ViewGrid_Click(object sender, EventArgs e)
        {
            if (activeScene != null)
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
            OrbitMenuItem.Checked = true;
            FpsMenuItem.Checked = false;
            ActiveCamera = new CameraOrbit(Vector3.UnitZ * 5f);
        }

        private void CameraFPS_Click(object sender, EventArgs e)
        {
            OrbitMenuItem.Checked = false;
            FpsMenuItem.Checked = true;
            ActiveCamera = new CameraFPS(Vector3.UnitZ * 5f)
            {
                Yaw = -90f,
            };
        }

        private void InspectionMenuItem_Click(object sender, EventArgs e)
        {
            Inspection inspection = new Inspection(activeScene.RawScene);
            inspection.Show();
        }


        public static List<PointLight> pointLights = new List<PointLight>
        {
            new PointLight()
            {
                position = new Vector3(8.0f, 5.0f, 8.0f),
                diffuse = new Vector3(50f),
                ambient = new Vector3(4.0f),
                specular = new Vector3(90.0f),
                linear = 10.0027f,
                quadratic = 0.0028f
            },
            new PointLight()
            {
                position = new Vector3(-8.0f, 5.0f, 8.0f),
                diffuse = new Vector3(50f),
                ambient = new Vector3(4.0f),
                specular = new Vector3(90.0f),
                linear = 10.0027f,
                quadratic = 0.0028f
            },
            new PointLight()
            {
                position = new Vector3(-8.0f, 5.0f, -8.0f),
                diffuse = new Vector3(50f),
                ambient = new Vector3(4.0f),
                specular = new Vector3(90.0f),
                linear = 10.0027f,
                quadratic = 0.0028f
            },
            new PointLight()
            {
                position = new Vector3(8.0f, 5.0f, -8.0f),
                diffuse = new Vector3(50f),
                ambient = new Vector3(4.0f),
                specular = new Vector3(90.0f),
                linear = 10.0027f,
                quadratic = 0.0028f
            },
        };

        private void OpenFile(string filename)
        {
            try
            {
                if (activeScene != null) activeScene.Dispose();
                activeScene = new Model(filename);

                // Camera
                CameraOrbit_Click(null, null);
                if (ActiveCamera != null) ActiveCamera.SetTarget(activeScene.pivot * activeScene.scale);
            }
            catch (Exception)
            {
            }

        }

        private void ApplicationIdle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                FrameUpdate();
                FrameRender();
            }
        }

        private void FrameUpdate()
        {
            fpsTracker.Update();

            if (GridProperties.Dirty)
            {
                grid2D = new Grid2D();
                GridProperties.Dirty = false;
            }
            if (ViewFPSMenuItem.Checked) fpsTracker.GetFps(labelFPS);

            if (ActiveCamera != null) ActiveCamera.MovementKey((float)fpsTracker.LastFrameDelta);

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

                activeScene.renderer.Render(view, perspective);

                if (ViewEnvironmentMenuItem.Checked) environment.Render(view, perspective);

                if (ViewGridMenuItem.Checked) grid2D.Render(view, perspective);

                if (ViewFPSMenuItem.Checked) labelFPS.Render();

                glControl1.SwapBuffers();
            }
        }

        #region Camera

        private int _previousMousePosX = -1;
        private int _previousMousePosY = -1;

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if(ActiveCamera == null) return;
            if (e.Delta != 0)
                ActiveCamera.Scroll(e.Delta);
            if(e.Button == MouseButtons.Middle)
                ActiveCamera.Pan(e.X - _previousMousePosX, e.Y - _previousMousePosY);
            if(e.Button == MouseButtons.Left)
                ActiveCamera.MouseMove(e.X - _previousMousePosX, e.Y - _previousMousePosY);

            _previousMousePosX = e.X;
            _previousMousePosY = e.Y;
        }

        #endregion



    }
}
