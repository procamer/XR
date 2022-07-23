using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.IO;

namespace XR
{
    public class Renderer
    {
        private Shader shader;

        private Scene scene;
        private List<Mesh> meshes = new List<Mesh>();
        private Animator animator;
        internal int time;
        private TextureSet textureSet;
        public Matrix4 modelMatrix;
        public Vector3 pivot;
        public float scale;

        public Renderer(Scene scene, List<Mesh> meshes, Animator animator, Matrix4 modelMatrix, Vector3 pivot, float scale, string file)
        {
            this.scene = scene;
            this.meshes = meshes;
            this.animator = animator;
            this.modelMatrix = modelMatrix;
            this.pivot = pivot;
            this.scale = scale;
            textureSet = new TextureSet(Path.GetDirectoryName(file));
            shader = new Shader(@"Shaders/XRModelVert.glsl", @"Shaders/XRModelFrag.glsl");

            LoadTextures();
        }

        private void LoadTextures()
        {
            if (scene.Materials == null) { return; }

            foreach (Material material in scene.Materials)
            {
                TextureSlot[] textures = material.GetAllMaterialTextures();

                foreach (TextureSlot tex in textures)
                {
                    string path = tex.FilePath;
                    EmbeddedTexture embeddedSource = null;

                    if (path != null)
                    {
                        if (path.StartsWith("*"))
                        {
                            if (scene.HasTextures && uint.TryParse(path.Substring(1), out uint index) && index < scene.TextureCount)
                            {
                                embeddedSource = scene.Textures[(int)index];
                            }
                        }
                        textureSet.Add(tex.FilePath, embeddedSource);
                    }
                }

                // textureDiffuse
                if (material.GetMaterialTextureCount(TextureType.Diffuse) > 0)
                {
                    material.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot tex);
                    Texture texture = textureSet.GetTexture(tex.FilePath);
                    texture.UploadTexture(TextureUnit.Texture0);
                }

                // textureSpecular
                if (material.GetMaterialTextureCount(TextureType.Specular) > 0)
                {
                    material.GetMaterialTexture(TextureType.Specular, 0, out TextureSlot tex);
                    Texture texture = textureSet.GetTexture(tex.FilePath);
                    texture.UploadTexture(TextureUnit.Texture1);
                }

                // textureHeight
                if (material.GetMaterialTextureCount(TextureType.Height) > 0)
                {
                    material.GetMaterialTexture(TextureType.Height, 0, out TextureSlot tex);
                    Texture texture = textureSet.GetTexture(tex.FilePath);
                    texture.UploadTexture(TextureUnit.Texture2);
                }
            }
        }


        public void Render(Matrix4 view, Matrix4 perspective)
        {
            if (meshes == null) return;

            shader.Use();

            // lights
            for (int i = 0; i < MainWindow.pointLights.Count; i++)
                MainWindow.pointLights[i].Set(shader, i);

            // matrixes
            shader.SetVec3("viewPos", new Vector3(view.M41, view.M42, view.M43));
            shader.SetMat4("viewMatrix", view);
            shader.SetMat4("projectionMatrix", perspective);


            if (scene.HasAnimations)
            {
                shader.SetInt("hasAnimations", 1);
            }
            else
            {
                shader.SetInt("hasAnimations", 0);
            }

            time++;
            for (int i = 0; i < meshes.Count; i++)
            {
                ApplyModelMatrix(shader, i);
                ApplyMaterial(shader, scene.Materials[meshes[i].MaterialIndex]);
                if (scene.HasAnimations)
                {
                    animator.bones = meshes[i].boneTransforms;
                    animator.UpdateAnimation(time / 90f, 0);
                    for (int j = 0; j < animator.bones.Count; j++)
                        shader.SetMat4("boneTransform[" + j + "]", animator.bones[j].Transformation);
                }
                meshes[i].Draw(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles);
            }
        }

        private void ApplyModelMatrix(Shader shader, int i)
        {
            Matrix4 model =
                meshes[i].transform *
                modelMatrix *
                Matrix4.CreateTranslation(pivot) *
                Matrix4.CreateScale(scale);

            shader.SetMat4("modelMatrix", model);

        }

        private void ApplyMaterial(Shader shader, Material material)
        {
            // colorDiffuse
            Vector3 colorDiffuse = new Vector3(1.0f, 0.5f, 0.31f);
            if (material.HasColorDiffuse)
                colorDiffuse = Utility.FromColor4DtoVector3(material.ColorDiffuse);
            shader.SetVec3("colorDiffuse", colorDiffuse);

            // textureDiffuse (Texture0)
            if (material.HasTextureDiffuse)
            {
                material.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texture.ID);
                shader.SetInt("textureDiffuse", 0);
                shader.SetInt("hasTextureDiffuse", 1); // true
            }
            else
            {
                shader.SetInt("hasTextureDiffuse", 0); // false
            }

            // colorSpecular
            Vector3 colorSpecular = new Vector3(0.7f, 0.7f, 0.7f);
            if (material.HasColorSpecular)
                colorSpecular = Utility.FromColor4DtoVector3(material.ColorSpecular);
            shader.SetVec3("colorSpecular", colorSpecular);

            // textureSpecular (Texture1)
            if (material.HasTextureSpecular)
            {
                material.GetMaterialTexture(TextureType.Specular, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                if (texture != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, texture.ID);
                    shader.SetInt("textureSpecular", 1);
                    shader.SetInt("hasTextureSpecular", 1);
                }
            }
            else
            {
                shader.SetInt("hasTextureSpecular", 0);
            }

            // textureHeight (Texture2)
            if (material.HasTextureHeight)
            {
                material.GetMaterialTexture(TextureType.Height, 0, out TextureSlot tex);
                Texture texture = textureSet.GetTexture(tex.FilePath);
                if (texture != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, texture.ID);
                    shader.SetInt("textureNormal", 2);
                    shader.SetInt("hasTextureNormal", 1);
                }
            }
            else
            {
                shader.SetInt("hasTextureNormal", 0);
            }

            // Clear textures
            GL.Disable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

    }
}
