using Assimp;
using Assimp.Configs;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XR
{
    public sealed class Model : IDisposable
	{
        private Scene scene;
        private Animator animator;
		private List<Mesh> meshes = new List<Mesh>();		
        public Renderer renderer;

        public Matrix4 modelMatrix;
        public Vector3 sceneCenter;
        public Vector3 sceneMin;
        public Vector3 sceneMax;
        public Vector3 pivot;
        public float scale;

        public Scene RawScene => scene;

        public Model(string file)
		{
			try
			{
				using (AssimpContext context = new AssimpContext())
				{
					context.SetConfig(new NormalSmoothingAngleConfig(66.0f));

					PostProcessSteps postprocess = Utility.GetPostProcessStepsFlags();
					scene = context.ImportFile(file, postprocess);
					if (scene == null)
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

            ComputeBoundingBox(out sceneMin, out sceneMax, out sceneCenter);
			
            float tmp = sceneMax.X - sceneMin.X;
            tmp = Math.Max(sceneMax.Y - sceneMin.Y, tmp);
            tmp = Math.Max(sceneMax.Z - sceneMin.Z, tmp);
            scale = 2.0f / tmp;            
			pivot = Vector3.Multiply(sceneCenter, -1.0f);			
			modelMatrix = scene.RootNode != null ? Utility.FromMatrix4x4T(scene.RootNode.Transform) : Matrix4.Identity;

            RecursiveNode(scene.RootNode);

            animator = new Animator(scene.Animations, scene.RootNode);            
            renderer = new Renderer(scene, meshes, animator, modelMatrix, pivot, scale, file);            
        }

        private void RecursiveNode(Node node)
		{
			for (int i = 0; i < node.MeshCount; i++)
			{
				Mesh mesh = ProcessMesh(scene.Meshes[node.MeshIndices[i]]);
				mesh.transform = node.Transform != null ? Utility.FromMatrix4x4T(node.Transform) : Matrix4.Identity ;				
				meshes.Add(mesh);
			}

			for (int i = 0; i < node.ChildCount; i++)
			{
				RecursiveNode(node.Children[i]);
			}
		}

		private Mesh ProcessMesh(Assimp.Mesh mesh)
		{
			Mesh result = new Mesh {MaterialIndex = mesh.MaterialIndex};

			// vertices
			for (int i = 0; i < mesh.VertexCount; i++)
			{
                Vertex vertex = new Vertex()
                {
                    position = Utility.FromVector3Dto3(mesh.Vertices[i]),
                    normal = Utility.FromVector3Dto3(mesh.Normals[i]),

                };

                if (mesh.Tangents .Count == mesh.VertexCount)
                {
                    vertex.tangent = Utility.FromVector3Dto3(mesh.Tangents[i]);
                    vertex.bitangent = Utility.FromVector3Dto3(mesh.BiTangents[i]);
                }

                vertex.texCoord = mesh.HasTextureCoords(0) ?
                    Utility.FromVector3Dto2(mesh.TextureCoordinateChannels[0][i]) :
                    Vector2.Zero;

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
            List<BoneT> boneTransforms = new List<BoneT>();
            for (int b = 0; b < mesh.BoneCount; b++)
            {
                // BoneTransform
                Assimp.Bone bone = mesh.Bones[b];
                boneTransforms.Add(new BoneT(bone.Name, Utility.FromMatrix4x4T(bone.OffsetMatrix)));

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
            result.UploadBuffers();
			return result;
		}

        private void ComputeBoundingBox(
            out Vector3 sceneMin, out Vector3 sceneMax, out Vector3 sceneCenter,
            Node node = null, bool omitRootNodeTrafo = false)
        {
            sceneMin = new Vector3(1e10f, 1e10f, 1e10f);
            sceneMax = new Vector3(-1e10f, -1e10f, -1e10f);
            Matrix4 transform = omitRootNodeTrafo ? Matrix4.Identity : Utility.FromMatrix4x4T((node ?? scene.RootNode).Transform);

            ComputeBoundingBox(node ?? scene.RootNode, ref sceneMin, ref sceneMax, ref transform);
            sceneCenter = (sceneMin + sceneMax) / 2.0f;
        }

        private void ComputeBoundingBox(
            Node node, ref Vector3 min, ref Vector3 max, ref Matrix4 transform)
        {
            if (node.HasMeshes)
            {
                foreach (Assimp.Mesh mesh in node.MeshIndices.Select(index => scene.Meshes[index]))
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

        public void Dispose()
		{
			if (scene != null) scene = null;
			meshes = null;
			GC.SuppressFinalize(this);
		}
	}
}
