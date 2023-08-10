using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace XR
{
    public class Environment
    {
        private Shader shader;
        private readonly Texture texture;
        private readonly int VBO, VAO;
        readonly float[] vertices =
            {
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,

                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,
            };

        public Environment()
        {
            shader = new Shader("Resources/Shaders/EnvironmentVert.glsl", "Resources/Shaders/EnvironmentFrag.glsl");
            texture = new Texture(Settings.Graphics.Default.FacesColl);

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

        }

        public void Render(Matrix4 view, Matrix4 perspective)
        {
            shader.Use();
            shader.SetMat4("viewMatrix", new Matrix4(new Matrix3(view)));
            shader.SetMat4("projectionMatrix", perspective);
            shader.SetInt("cubeTexture", 0);

            GL.DepthFunc(DepthFunction.Lequal);
            GL.BindVertexArray(VBO);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture.ID);
            GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length);
            GL.BindVertexArray(0);
            GL.DepthFunc(DepthFunction.Less);
        }
    }
}
