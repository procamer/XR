using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace XR
{
    public class Grid2D
    {
        private List<Vector2> vertices = new List<Vector2>();
        private int VAO, VBO;

        private Shader shader;

        public Grid2D()
        {
            shader = new Shader(@"Resources/Shaders/Grid2DVert.glsl", @"Resources/Shaders/Grid2DFrag.glsl");
            Update();
        }

        public void Update()
        {

            int horizontalDivisions = Settings.Properties.Default.GridHorizontalDivisions;
            int verticalDivisions = Settings.Properties.Default.GridVerticalDivisions;
            float cellSize = Settings.Properties.Default.GridCellSize;

            float width = horizontalDivisions * cellSize;
            float height = verticalDivisions * cellSize;
            float xMin = -width / 2;
            float yMin = -height / 2;

            float x = xMin;
            for (int i = 0; i < horizontalDivisions + 1; i++)
            {
                vertices.Add(new Vector2(x, yMin));
                vertices.Add(new Vector2(x, -yMin));
                x += cellSize;
            }

            float y = yMin;
            for (int i = 0; i < verticalDivisions + 1; i++)
            {
                vertices.Add(new Vector2(xMin, y));
                vertices.Add(new Vector2(-xMin, y));
                y += cellSize;
            }

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vector2.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);

            // positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);
            GL.BindVertexArray(0);
        }

        public void Render(Matrix4 view, Matrix4 perspective)
        {
            shader.Use();
            shader.SetMat4("projectionMatrix", perspective);
            shader.SetMat4("viewMatrix", view);
            shader.SetVec4("objectColor", Utility.GetGridColor());

            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Lines, 0, vertices.Count);
            GL.BindVertexArray(0);
        }

    }

    public class GridProperties
    {
        public static bool Dirty { get; set; }

        private Color _color;
        private int _width;
        private int _height;
        private float _cellSize;

        [Category("Color"), Description("Color of the grid")]
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                Settings.Properties.Default.GridColor = _color;
                Dirty = true;
            }
        }

        [Category("Size"), Description("Size of the grid")]
        [DisplayName("Horizontal Divisions")]
        public int HorizontalDivisions
        {
            get
            {
                return _width;
            }
            set
            {
                if (value > 100) value = 100;
                if (value < 1) value = 1;
                _width = value;
                Settings.Properties.Default.GridHorizontalDivisions = _width;
                Dirty = true;
            }
        }

        [Category("Size"), Description("Size of the grid")]
        [DisplayName("Vertical Divisions")]
        public int VerticalDivisions
        {
            get
            {
                return _height;
            }
            set
            {
                if (value > 100) value = 100;
                if (value < 1) value = 1;
                _height = value;
                Settings.Properties.Default.GridVerticalDivisions = _height;
                Dirty = true;
            }
        }

        [Category("Size"), Description("Size of the grid")]
        [DisplayName("Cell Size")]
        public float CellSize
        {
            get
            {
                return _cellSize;
            }
            set
            {
                if (value > 100f) value = 100f;
                if (value < 0.1f) value = 0.1f;
                _cellSize = value;
                Settings.Properties.Default.GridCellSize = _cellSize;
                Dirty = true;
            }
        }


    }

}
