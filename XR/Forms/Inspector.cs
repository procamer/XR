using Assimp;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace XR
{
    public partial class Inspector : Form
    {

        public bool Playing
        {
            get { return _playing; }
            set
            {
                if (value == _playing) return;
                _playing = value;
                if (value)
                {
                    StartPlayingTimer();
                    _sceneAnimator.AnimationPlaybackSpeed = _sceneAnimator.ActiveAnimation >= 0 ? AnimPlaybackSpeed : 0.0;
                }
                else
                {
                    StopPlayingTimer();
                    _sceneAnimator.AnimationPlaybackSpeed = 0.0;
                }
            }
        }

        private Assimp.Animation ActiveAnimation
        {
            get
            {
                return listBoxAnimations.SelectedIndex == 0
                    ? null
                    : _scene.Raw.Animations[listBoxAnimations.SelectedIndex - 1];
            }
        }

        public double AnimPlaybackSpeed
        {
            get { return _animPlaybackSpeed; }
            private set
            {
                Debug.Assert(value > 1e-6, "use Playing=false to not play animations");
                _animPlaybackSpeed = value;

                // avoid float noise close to 1
                if (Math.Abs(_animPlaybackSpeed - 1) < 1e-7)
                {
                    _animPlaybackSpeed = 1.0;
                }

                if (_playing)
                {
                    _sceneAnimator.AnimationPlaybackSpeed = AnimPlaybackSpeed;
                }

                //BeginInvoke(new MethodInvoker(() =>
                //{
                //    labelSpeedValue.Text = string.Format("{0}x", _animPlaybackSpeed.ToString("0.00"));
                //}));
            }
        }

        private Scene _scene;
        private SceneAnimator _sceneAnimator;
        private double _animPlaybackSpeed = 1.0;
        private int _speedAdjust;
        private const int MaxSpeedAdjustLevels = 8;
        private Timer _timer;
        private bool _playing;
        private const int TimerInterval = 30;
        private const double PlaybackSpeedAdjustFactor = 0.6666;

        public Inspector(Scene scene)
        {
            InitializeComponent();
            _scene = scene;
            _sceneAnimator = scene.SceneAnimator;
            
            activeScene = _scene.Raw;
            AddNodes();

        }

        private void Animation_Load(object sender, EventArgs e)
        {
            listBoxAnimations.Items.Add("None (Bind Pose)");
            listBoxAnimations.SelectedIndex = 0;
            if (_scene.Raw.Animations != null)
            {
                foreach (var anim in _scene.Raw.Animations)
                {
                    listBoxAnimations.Items.Add(FormatAnimationName(anim));
                }
            }

            checkBoxLoop.Checked = _sceneAnimator.Loop;
            buttonPlay.Image = imageListTree.Images[0];

            timeSlideControl.Rewind += (o, args) =>
            {
                if (_sceneAnimator.ActiveAnimation >= 0)
                {
                    _sceneAnimator.Cursor = args.NewPosition;
                    UpdateDisplay();
                }
            };
        }

        private void listBoxAnimations_SelectedIndexChanged(object sender, EventArgs e)
        {
            _sceneAnimator.ActiveAnimation = listBoxAnimations.SelectedIndex - 1;

            if (_sceneAnimator.ActiveAnimation >= 0 && ActiveAnimation.DurationInTicks > 0.0)
            {
                foreach (var control in panelAnimTools.Controls)
                {
                    if (control == buttonSlower && _speedAdjust == -MaxSpeedAdjustLevels ||
                        control == buttonFaster && _speedAdjust == MaxSpeedAdjustLevels)
                    {
                        continue;
                    }
                    ((Control)control).Enabled = true;
                }

                _sceneAnimator.Cursor = 1 / ActiveAnimation.TicksPerSecond;
                _sceneAnimator.AnimationPlaybackSpeed = 0.0;

                timeSlideControl.RangeMin = 0.0;
                timeSlideControl.RangeMax = _sceneAnimator.AnimationDuration;

                hScrollCursor.Minimum = 0;
                hScrollCursor.Maximum = (int)_sceneAnimator.DurationInTicks;

                StartPlayingTimer();
            }
            else
            {
                foreach (var control in panelAnimTools.Controls)
                {
                    ((Control)control).Enabled = false;
                }
                StopPlayingTimer();
            }
            Playing = false;
            buttonPlay.Image = imageListTree.Images[0];
            if (_sceneAnimator.ActiveAnimation != -1) UpdateDisplay();
        }

        private void listBoxAnimations_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Playing = true;
            buttonPlay.Image = Playing ? imageListTree.Images[1] : imageListTree.Images[0];
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            Playing = !Playing;
            buttonPlay.Image = Playing ? imageListTree.Images[1] : imageListTree.Images[0];
        }

        private void buttonSlower_Click(object sender, EventArgs e)
        {
            Debug.Assert(_speedAdjust > -MaxSpeedAdjustLevels);
            if (--_speedAdjust == -MaxSpeedAdjustLevels)
            {
                buttonSlower.Enabled = false;
            }
            buttonFaster.Enabled = true;
            AnimPlaybackSpeed *= PlaybackSpeedAdjustFactor;
            UpdateDisplay();
        }

        private void buttonFaster_Click(object sender, EventArgs e)
        {
            Debug.Assert(_speedAdjust < MaxSpeedAdjustLevels);
            if (++_speedAdjust == MaxSpeedAdjustLevels)
            {
                buttonFaster.Enabled = false;
            }
            buttonSlower.Enabled = true;
            AnimPlaybackSpeed /= PlaybackSpeedAdjustFactor;
            UpdateDisplay();
        }

        private void checkBoxLoop_CheckedChanged(object sender, EventArgs e)
        {
            _sceneAnimator.Loop = checkBoxLoop.Checked;
        }

        private void hScrollCursor_Scroll(object sender, ScrollEventArgs e)
        {
            _sceneAnimator.Cursor = (hScrollCursor.Value / _sceneAnimator.TicksPerSecond);
            UpdateDisplay();
        }

        private void StopPlayingTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        private void StartPlayingTimer()
        {
            if (!Playing) return;
            // we can get called by change animation without stopping, in which case only create a timer if there
            // isn't one there already - MAN.
            if (_timer == null)
            {
                _timer = new Timer { Interval = (TimerInterval) };
                _timer.Tick += (o, args) =>
                {
                    UpdateDisplay();
                };
                _timer.Start();
            }
        }

        private static string FormatAnimationName(Assimp.Animation anim)
        {
            var dur = anim.DurationInTicks;
            if (anim.TicksPerSecond > 1e-10)
            {
                dur /= anim.TicksPerSecond;
            }
            else
            {
                dur /= SceneAnimator.DefaultTicksPerSecond;
            }
            string text = string.Format("{0} ({1}s)", anim.Name, dur.ToString("0.000"));
            return text;
        }

        private void UpdateDisplay()
        {
            timeSlideControl.Position = _sceneAnimator.Cursor;

            hScrollCursor.Value = MathHelper.Clamp(
                (int)Math.Round(_sceneAnimator.Cursor * _sceneAnimator.TicksPerSecond),
                hScrollCursor.Minimum,
                hScrollCursor.Maximum);

            label1.Text =
                "Active Animation : " + _sceneAnimator.ActiveAnimation.ToString() + "\n" +
                "TicksPerSecond : " + _sceneAnimator.TicksPerSecond.ToString("0") + "\n" +
                "Number of Frames : " + _sceneAnimator.DurationInTicks.ToString() + "\n" +                
                "Anim Playback Speed : " + _animPlaybackSpeed.ToString("0.00x") + "\n" +
                "AnimationDuration : " + (_sceneAnimator.AnimationDuration * (1/AnimPlaybackSpeed)).ToString("0.000s") + "\n" +
                "\n" +                
                "Cursor : " + _sceneAnimator.Cursor.ToString("0.000s") + "\n" +                
                "Current Frame : " + (_sceneAnimator.Cursor * _sceneAnimator.TicksPerSecond).ToString("0");
        }

        // Inspection------------------------------------------

        private const int AutoExpandLevels = 4;
        private readonly Dictionary<Node, NodePurpose> _nodePurposes =
            new Dictionary<Node, NodePurpose>();
        private readonly Dictionary<KeyValuePair<Node, Assimp.Mesh>, TreeNode> _treeNodesBySceneNodeMeshPair =
            new Dictionary<KeyValuePair<Node, Assimp.Mesh>, TreeNode>();
        private Assimp.Scene activeScene;

        public enum NodePurpose
        {
            // note: this maps one by one to the TreeView's image indices
            Joint = 2,
            ImporterGenerated = 0,
            GenericMeshHolder = 1,

            Camera = 3,
            Light = 4
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
