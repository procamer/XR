using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XR
{
    public class Shader : IDisposable
    {
        const string ShaderVersion = "#version 450 core";

        private readonly int program;

        private Dictionary<string, int> uniformLocations;

        public Shader(string vertexShaderPath, string fragmentShaderPath, string geometryShaderPath = "",
                                string vertexDefines = "", string fragmentDefines = "")
        {
            int vertexShader, fragShader, geometryShader = 0;
            string result;

            program = GL.CreateProgram();

            // vertex
            StreamReader reader = new StreamReader(vertexShaderPath, Encoding.UTF8);
            result = string.Format(ShaderVersion + "\n{0}\n{1}", vertexDefines, reader.ReadToEnd());
            vertexShader = GetID(result, ShaderType.VertexShader);
            GL.AttachShader(program, vertexShader);

            // fragment
            reader = new StreamReader(fragmentShaderPath, Encoding.UTF8);
            result = string.Format(ShaderVersion + "\n{0}\n{1}", fragmentDefines, reader.ReadToEnd());
            fragShader = GetID(result, ShaderType.FragmentShader);
            GL.AttachShader(program, fragShader);

            // geometry
            if (!string.IsNullOrEmpty(geometryShaderPath))
            {
                geometryShader = GetID(geometryShaderPath, ShaderType.GeometryShader);
                GL.AttachShader(program, geometryShader);
            }
            // link
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == -1)
            {
                GL.GetProgramInfoLog(program, out string infoLog);
                throw new Exception($"shader programı bağlantılı değil: {infoLog}");
            }
            // clear
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragShader);
            if (!string.IsNullOrEmpty(geometryShaderPath)) GL.DeleteShader(geometryShader);
        }

        private int GetID(string result, ShaderType shaderType)
        {
            int id = GL.CreateShader(shaderType);

            GL.ShaderSource(id, result);
            GL.CompileShader(id);

            GL.GetShader(id, ShaderParameter.CompileStatus, out int success);
            if (success == -1)
            {
                GL.GetShaderInfoLog(id, out string infoLog);
                Dispose();
                throw new InvalidDataException(infoLog);
            }

            return id;
        }

        private void GetUniforms()
        {
            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            uniformLocations = new Dictionary<string, int>();
            for (int i = 0; i < numberOfUniforms; i++)
            {
                string key = GL.GetActiveUniform(program, i, out _, out _);
                int location = GL.GetUniformLocation(program, key);
                uniformLocations.Add(key, location);
            }
        }

        private void Debug(int shader)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            if (infoLog != string.Empty) Console.WriteLine(infoLog);
        }

        public void Use()
        {
            GL.UseProgram(program);
        }

        public void SetInt(string name, int value)
        {
            GL.Uniform1(GL.GetUniformLocation(program, name), value);
        }

        public void SetFloat(string name, float value)
        {
            GL.Uniform1(GL.GetUniformLocation(program, name), value);
        }

        public void SetVec3(string name, Vector3 vector)
        {
            SetVec3(name, vector.X, vector.Y, vector.Z);
        }

        public void SetVec3(string name, float x, float y, float z)
        {
            GL.Uniform3(GL.GetUniformLocation(program, name), x, y, z);
        }

        public void SetVec4(string name, Vector4 vector)
        {
            SetVec4(name, vector.X, vector.Y, vector.Z, vector.W);
        }

        public void SetVec4(string name, float x, float y, float z, float w)
        {
            GL.Uniform4(GL.GetUniformLocation(program, name), x, y, z, w);
        }

        public void SetMat4(string name, Matrix4 matrix)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(program, name), false, ref matrix);
        }

        public void Dispose()
        {
            GL.DeleteProgram(program);
            GC.SuppressFinalize(this);
        }

    }
}
