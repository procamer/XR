using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace XR
{
    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texCoord;
        public Vector3 tangent;
        public Vector3 bitangent;
        public Vector4 boneID;
        public Vector4 boneWeight;

        public static int SizeInBytes => Vector3.SizeInBytes * 4 + Vector2.SizeInBytes + Vector4.SizeInBytes * 2;
    }

    public class Mesh
    {
        private int VAO, VBO, EBO;
        private int indicesCount;
        public Matrix4 transform = Matrix4.Identity;
        public List<Vertex> vertices = new List<Vertex>();
        public List<uint> indices = new List<uint>();
        public List<NodeState> boneTransforms = new List<NodeState>();

        public int MaterialIndex { get; set; }

        public void UploadBuffers()
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vertex.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

            // Positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);
            // Normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 3);
            // TexCoords
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 6);
            // Tangents
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 8);
            // Bitangents
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 11);
            // BoneID
            GL.EnableVertexAttribArray(5);
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 14);
            // BoneWeight
            GL.EnableVertexAttribArray(6);
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, sizeof(float) * 18);

            indicesCount = indices.Count;
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Draw(PrimitiveType primitiveType)
        {
            GL.BindVertexArray(VAO);
            GL.DrawElements(primitiveType, indicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

    }

}
