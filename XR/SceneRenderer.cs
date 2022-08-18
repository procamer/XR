using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace XR
{
    public class SceneRenderer
    {
        private readonly Scene _scene;
        private readonly Shader shader;

        public SceneRenderer(Scene scene)
        {
            _scene = scene;
            string vertexDefines = string.Format("uniform mat4 boneTransform[{0}];", scene.SceneAnimator.MaxBoneCount);
            shader = new Shader(@"Resources/Shaders/ModelVert.glsl", @"Resources/Shaders/ModelFrag.glsl", "", vertexDefines);
        }

        public void Render(Matrix4 view, Matrix4 perspective)
        {
            if (_scene._meshes == null) return;

            shader.Use();

            // lights
            for (int i = 0; i < MainWindow.pointLights.Count; i++)
                MainWindow.pointLights[i].Set(shader, i);

            // matrices
            shader.SetVec3("viewPos", new Vector3(view.M41, view.M42, view.M43));
            shader.SetMat4("viewMatrix", view);
            shader.SetMat4("projectionMatrix", perspective);

            if (_scene.Raw.HasAnimations)
            {
                shader.SetInt("hasAnimations", 1);
            }
            else
            {
                shader.SetInt("hasAnimations", 0);
            }

            for (int i = 0; i < _scene._meshes.Count; i++)
            {
                // modelMatrix
                Matrix4 model = _scene._meshes[i].transform * _scene._modelMatrix;
                shader.SetMat4("modelMatrix", model);

                ApplyMaterial(shader, _scene.Raw.Materials[_scene._meshes[i].MaterialIndex]);

                if (_scene.Raw.HasAnimations && _scene.SceneAnimator.ActiveAnimation > -1)
                {
                    shader.SetInt("hasAnimations", 1);
                    _scene.SceneAnimator._bones = _scene._meshes[i].boneTransforms;
                    _scene.SceneAnimator.UpdateAnimation();
                    for (int j = 0; j < _scene.SceneAnimator._bones.Count; j++)
                        shader.SetMat4("boneTransform[" + j + "]", _scene.SceneAnimator._bones[j].Transformation);
                }
                else
                {
                    shader.SetInt("hasAnimations", 0);
                }
                _scene._meshes[i].Draw(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles);
            }
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
                Texture texture = _scene.TextureSet.GetTexture(tex.FilePath);
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
                Texture texture = _scene.TextureSet.GetTexture(tex.FilePath);
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
                Texture texture = _scene.TextureSet.GetTexture(tex.FilePath);
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
