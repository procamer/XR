using Assimp;
using Assimp.Configs;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XR
{
    public sealed class Scene : IDisposable
    {
        public Assimp.Scene Raw => _raw;
        public SceneAnimator SceneAnimator => _animator;
        public SceneRenderer Renderer => _renderer;
        public TextureSet TextureSet => _textureSet;

        public List<Mesh> _meshes = new List<Mesh>();
        public Matrix4 _modelMatrix;
        public Vector3 _pivot;
        public float _scale;

        private readonly Assimp.Scene _raw;
        private readonly SceneAnimator _animator;
        private readonly SceneRenderer _renderer;
        private readonly TextureSet _textureSet;
        private readonly Vector3 sceneCenter;
        private readonly Vector3 sceneMin;
        private readonly Vector3 sceneMax;

        private double _accumulatedTimeDelta;

        public Scene(string file)
        {
            // Dosyadaki sahneyi _rawa yüklemekle işe başlıyoruz. 
            try
            {
                using (AssimpContext context = new AssimpContext())
                {
                    context.SetConfig(new NormalSmoothingAngleConfig(66.0f));

                    PostProcessSteps postprocess = Utility.GetPostProcessStepsFlags();
                    _raw = context.ImportFile(file, postprocess);
                    if (_raw == null)
                    {
                        Dispose();
                        throw new Exception("failed to read file: " + file);
                    }
                }
            }
            catch (AssimpException e)
            {
                Dispose();
                throw new Exception("failed to read file: " + file + " (" + e.Message + ")");
            }

            // Model bilgisini GPU ya yüklüyoruz. Model bilgisi vertexlerden indekslerden ve kemik yapısından oluşuyor.
            RecursiveNode(_raw.RootNode);

            _animator = new SceneAnimator(_raw);

            _textureSet = new TextureSet(Path.GetDirectoryName(file));
            LoadTextures();

            ComputeBoundingBox(out sceneMin, out sceneMax, out sceneCenter);
            _pivot = sceneMin;

            float tmp = sceneMax.X - sceneMin.X;
            tmp = Math.Max(sceneMax.Y - sceneMin.Y, tmp);
            tmp = Math.Max(sceneMax.Z - sceneMin.Z, tmp);
            _scale = 2.0f / tmp;
            _modelMatrix = _raw.RootNode != null ? Utility.FromMatrix4x4T(_raw.RootNode.Transform) : Matrix4.Identity;
            _modelMatrix *= Matrix4.CreateTranslation(-_pivot);
            _modelMatrix *= Matrix4.CreateScale(_scale);

            _renderer = new SceneRenderer(this);
        }

        private void RecursiveNode(Node node)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                Mesh mesh = ProcessMesh(_raw.Meshes[node.MeshIndices[i]]);
                mesh.transform = node.Transform != null ?
                    Utility.FromMatrix4x4T(node.Transform) :
                    Matrix4.Identity;
                _meshes.Add(mesh);
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RecursiveNode(node.Children[i]);
            }
        }

        private Mesh ProcessMesh(Assimp.Mesh mesh)
        {
            Mesh result = new Mesh();

            // vertices
            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex = new Vertex()
                {
                    position = Utility.FromVector3Dto3(mesh.Vertices[i]),
                    normal = Utility.FromVector3Dto3(mesh.Normals[i]),
                };

                if (mesh.HasTangentBasis && mesh.Tangents.Count == mesh.VertexCount)
                {
                    vertex.tangent = Utility.FromVector3Dto3(mesh.Tangents[i]);
                    vertex.bitangent = Utility.FromVector3Dto3(mesh.BiTangents[i]);
                }

                // Sadece 0 indeksli texture koordinatlarını alıyoruz. Her yerde geçerli. Diğer indeksler için list oluşturulabilir ama gerek duymadım.
                vertex.texCoord = mesh.HasTextureCoords(0)
                    ? Utility.FromVector3Dto2(mesh.TextureCoordinateChannels[0][i])
                    : Vector2.Zero;

                result.vertices.Add(vertex);
            }

            // indices
            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    result.indices.Add((uint)face.Indices[j]);
                }
            }

            // bones
            if (_raw.HasAnimations)
            {
                List<NodeState> boneTransforms = new List<NodeState>();
                for (int b = 0; b < mesh.BoneCount; b++)
                {
                    // BoneTransform
                    Bone bone = mesh.Bones[b];
                    boneTransforms.Add(new NodeState(bone.Name, Utility.FromMatrix4x4T(bone.OffsetMatrix)));

                    // VertexWeight
                    for (int w = 0; w < bone.VertexWeightCount; w++)
                    {
                        VertexWeight vw = bone.VertexWeights[w];
                        int access = vw.VertexID;
                        Vertex vertex = result.vertices[access];

                        if (result.vertices[access].boneID.X == 0 && result.vertices[access].boneWeight.X == 0)
                        {
                            vertex.boneID.X = b;
                            vertex.boneWeight.X = vw.Weight;
                            result.vertices[access] = vertex;
                        }
                        else if (result.vertices[access].boneID.Y == 0 && result.vertices[access].boneWeight.Y == 0)
                        {
                            vertex.boneID.Y = b;
                            vertex.boneWeight.Y = vw.Weight;
                            result.vertices[access] = vertex;
                        }
                        else if (result.vertices[access].boneID.Z == 0 && result.vertices[access].boneWeight.Z == 0)
                        {
                            vertex.boneID.Z = b;
                            vertex.boneWeight.Z = vw.Weight;
                            result.vertices[access] = vertex;
                        }
                        else
                        {
                            vertex.boneID.W = b;
                            vertex.boneWeight.W = vw.Weight;
                            result.vertices[access] = vertex;
                        }
                    }
                }
                result.boneTransforms = boneTransforms;
            }

            // material index
            if (_raw.HasMaterials)
                result.MaterialIndex = mesh.MaterialIndex;

            result.UploadBuffers();
            return result;
        }

        private void LoadTextures()
        {
            if (_raw.Materials == null) { return; }

            foreach (Material material in _raw.Materials)
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
                            if (_raw.HasTextures && uint.TryParse(path.Substring(1), out uint index) && index < _raw.TextureCount)
                            {
                                embeddedSource = _raw.Textures[(int)index];
                            }
                        }
                        _textureSet.Add(tex.FilePath, embeddedSource);
                    }
                }

                // textureDiffuse
                if (material.GetMaterialTextureCount(TextureType.Diffuse) > 0)
                {
                    material.GetMaterialTexture(TextureType.Diffuse, 0, out TextureSlot tex);
                    Texture texture = _textureSet.GetTexture(tex.FilePath);
                    texture.UploadTexture(TextureUnit.Texture0);
                }

                // textureSpecular
                if (material.GetMaterialTextureCount(TextureType.Specular) > 0)
                {
                    material.GetMaterialTexture(TextureType.Specular, 0, out TextureSlot tex);
                    Texture texture = _textureSet.GetTexture(tex.FilePath);
                    texture.UploadTexture(TextureUnit.Texture1);
                }

                // textureHeight
                if (material.GetMaterialTextureCount(TextureType.Height) > 0)
                {
                    material.GetMaterialTexture(TextureType.Height, 0, out TextureSlot tex);
                    Texture texture = _textureSet.GetTexture(tex.FilePath);
                    texture.UploadTexture(TextureUnit.Texture2);
                }
                //else if (material.GetMaterialTextureCount(TextureType.Normals) > 0)
                //{
                //    material.GetMaterialTexture(TextureType.Normals, 0, out TextureSlot tex);
                //    Texture texture = textureSet.GetTexture(tex.FilePath);
                //    texture.UploadTexture(TextureUnit.Texture2);
                //}
            }
        }

        private void ComputeBoundingBox(
            out Vector3 sceneMin, out Vector3 sceneMax, out Vector3 sceneCenter,
            Node node = null, bool omitRootNodeTrafo = false)
        {
            sceneMin = new Vector3(1e10f, 1e10f, 1e10f);
            sceneMax = new Vector3(-1e10f, -1e10f, -1e10f);
            Matrix4 transform = omitRootNodeTrafo ? Matrix4.Identity : Utility.FromMatrix4x4T((node ?? _raw.RootNode).Transform);

            ComputeBoundingBox(node ?? _raw.RootNode, ref sceneMin, ref sceneMax, ref transform);
            sceneCenter = (sceneMin + sceneMax) / 2.0f;
        }

        private void ComputeBoundingBox(
           Node node, ref Vector3 min, ref Vector3 max, ref Matrix4 transform)
        {
            if (node.HasMeshes)
            {
                foreach (Assimp.Mesh mesh in node.MeshIndices.Select(index => _raw.Meshes[index]))
                {
                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        Vector3 result = Vector3.Transform(
                            Utility.FromVector3Dto3(mesh.Vertices[i]),
                            new Matrix3(transform));

                        min.X = Math.Min(min.X, result.X);
                        min.Y = Math.Min(min.Y, result.Y);
                        min.Z = Math.Min(min.Z, result.Z);

                        max.X = Math.Max(max.X, result.X);
                        max.Y = Math.Max(max.Y, result.Y);
                        max.Z = Math.Max(max.Z, result.Z);
                    }
                }
            }

            for (var i = 0; i < node.ChildCount; i++)
            {
                Matrix4 prev = Matrix4.Mult(Utility.FromMatrix4x4T(node.Children[i].Transform), transform);
                ComputeBoundingBox(node.Children[i], ref min, ref max, ref prev);
            }
        }

        public void Update(double delta, bool silent = false)
        {
            if (silent)
            {
                _accumulatedTimeDelta += delta;
                return;
            }
            _animator.Update(delta + _accumulatedTimeDelta);
            _accumulatedTimeDelta = 0.0;
        }

        public void Dispose()
        {
            //if (_raw != null) _raw = null;
            _meshes = null;
            GC.SuppressFinalize(this);
        }
    }
}