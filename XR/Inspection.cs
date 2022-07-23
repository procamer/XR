using Assimp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace XR
{
    public partial class Inspection : Form
    {
        private const int AutoExpandLevels = 4;
        private readonly Dictionary<Node, NodePurpose> _nodePurposes =
            new Dictionary<Node, NodePurpose>();
        private readonly Dictionary<KeyValuePair<Node, Assimp.Mesh>, TreeNode> _treeNodesBySceneNodeMeshPair =
            new Dictionary<KeyValuePair<Node, Assimp.Mesh>, TreeNode>();
        private Scene activeScene;

        public enum NodePurpose
        {
            // note: this maps one by one to the TreeView's image indices
            Joint = 2,
            ImporterGenerated = 0,
            GenericMeshHolder = 1,

            Camera = 3,
            Light = 4
        }

        public Inspection(Scene scene)
        {
            InitializeComponent();
            activeScene = scene;
            AddNodes();
        }

        public void AddNodes()
        {
            treeView1.Nodes.Clear();
            treeView1.BeginUpdate();
            AddNodes(activeScene.RootNode, null, 0);
            treeView1.EndUpdate();
        }

        private bool AddNodes(Node node, TreeNode uiNode, int level)
        {
            Debug.Assert(node != null);

            // default node icon
            var purpose = NodePurpose.GenericMeshHolder;
            var isSkeletonNode = false;

            // Mark nodes introduced by assimp (i.e. nodes not present in the source file)
            if (node.Name.StartsWith("<") && node.Name.EndsWith(">") || level == 0)
            {
                purpose = NodePurpose.ImporterGenerated;
            }
            else
            {
                // First check if this node has assimp lights or cameras assigned.
                for (var i = 0; i < activeScene.CameraCount; ++i)
                {
                    if (activeScene.Cameras[i].Name == node.Name)
                    {
                        purpose = NodePurpose.Camera;
                        break;
                    }
                }
                if (purpose == NodePurpose.GenericMeshHolder)
                {
                    for (var i = 0; i < activeScene.LightCount; ++i)
                    {
                        if (activeScene.Lights[i].Name == node.Name)
                        {
                            purpose = NodePurpose.Light;
                            break;
                        }
                    }
                }
                if (purpose == NodePurpose.GenericMeshHolder)
                {
                    // Mark skeleton nodes (joints) with a special icon. The algorithm to
                    // detect them is easy: check whether if this node or any children 
                    // carry meshes. if not, assume this is a joint.
                    isSkeletonNode = node.MeshCount == 0;
                }
            }

            TreeNode newUiNode = new TreeNode(node.Name) { Tag = node };

            if (uiNode == null)
            {
                treeView1.Nodes.Add(newUiNode);
                Debug.Assert(level == 0);
            }
            else
            {
                uiNode.Nodes.Add(newUiNode);
            }

            // Add child nodes.
            if (node.Children != null)
            {
                foreach (Node c in node.Children)
                {
                    isSkeletonNode = AddNodes(c, newUiNode, level + 1) && isSkeletonNode;
                }
            }

            // Add mesh nodes.
            if (node.MeshCount != 0)
            {
                foreach (var m in node.MeshIndices)
                {
                    AddMeshNode(node, activeScene.Meshes[m], m, newUiNode);
                }
            }

            if (isSkeletonNode)
            {
                purpose = NodePurpose.Joint;
            }

            _nodePurposes.Add(node, purpose);
            // TODO(acgessler): Proper icons for lights and cameras.
            var index = (int)purpose;
            if (purpose == NodePurpose.Light || purpose == NodePurpose.Camera)
            {
                index = 1;
            }
            newUiNode.ImageIndex = newUiNode.SelectedImageIndex = index;

            if (level < AutoExpandLevels)
            {
                newUiNode.Expand();
            }

            return isSkeletonNode;
        }

        private void AddMeshNode(Node owner, Assimp.Mesh mesh, int id, TreeNode uiNode)
        {
            Debug.Assert(uiNode != null);
            Debug.Assert(mesh != null);

            // Meshes need not be named, in this case we number them
            var desc = GetMeshDisplayName(mesh, id);

            var key = new KeyValuePair<Node, Assimp.Mesh>(owner, mesh);
            var newUiNode = new TreeNode(desc)
            {
                Tag = key,
                ImageIndex = 3,
                SelectedImageIndex = 3,
            };

            uiNode.Nodes.Add(newUiNode);
            _treeNodesBySceneNodeMeshPair[key] = newUiNode;
        }

        private static string GetMeshDisplayName(Assimp.Mesh mesh, int id)
        {
            return "Mesh " + (!string.IsNullOrEmpty(mesh.Name)
                            ? ("\"" + mesh.Name + "\"")
                            : id.ToString(CultureInfo.InvariantCulture));
        }

        private void RecursiveTreeNode(Node node)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                treeView1.Nodes[0].Nodes.Add(new TreeNode(activeScene.Meshes[node.MeshIndices[i]].Name));
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RecursiveTreeNode(node.Children[i]);
            }
        }


    }
}
