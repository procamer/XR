using Assimp;
using OpenTK;
using System.Collections.Generic;
using System.Diagnostics;
using Quaternion = OpenTK.Quaternion;

namespace XR
{
    public class SceneAnimator
    {
        public const double DefaultTicksPerSecond = 30.0;

        public double TicksPerSecond
        {
            get
            {
                if (ActiveAnimation == -1) return 0.0;
                var anim = _raw.Animations[ActiveAnimation];
                return anim.TicksPerSecond > 1e-10 ? anim.TicksPerSecond : DefaultTicksPerSecond;
            }
        }

        public double DurationInTicks
        {
            get
            {
                if (ActiveAnimation == -1) return 0;
                var anim = _raw.Animations[ActiveAnimation];
                return anim.DurationInTicks;
            }
        }

        public double AnimationDuration
        {
            get
            {
                if (ActiveAnimation == -1) return 0.0;
                var anim = _raw.Animations[ActiveAnimation];
                return anim.DurationInTicks / TicksPerSecond;
            }
        }

        public int ActiveAnimation
        {
            get { return _activeAnim; }
            set
            {
                Debug.Assert(value >= -1 && value < _raw.AnimationCount);
                if (value == _activeAnim) return;
                _activeAnim = value;
            }
        }

        public double Cursor
        {
            get { return _activeFrameTime; }

            set
            {
                Debug.Assert(value >= 0);
                _activeFrameTime = value;

                if (_activeFrameTime > AnimationDuration)
                {
                    if (Loop)
                    {
                        if (AnimationDuration > 1e-6) _activeFrameTime %= AnimationDuration;
                    }
                    else
                    {
                        _activeFrameTime = AnimationDuration;
                    }
                }
                if (_activeFrameTime < 1 / (float)TicksPerSecond) _activeFrameTime = 1 / (float)TicksPerSecond;
            }
        }

        public bool Loop
        {
            get { return _loop; }
            set
            {
                _loop = value;
                if (value)
                {
                    var d = _activeFrameTime;
                    if (AnimationDuration > 1e-6) d %= AnimationDuration;
                    Cursor = d;
                }
            }
        }

        public int MaxBoneCount => _maxBoneCount;

        public double AnimationPlaybackSpeed
        {
            get { return _animPlaybackSpeed; }

            set
            {
                Debug.Assert(value >= 0);
                _animPlaybackSpeed = value;
            }
        }

        public List<NodeState> _bones;

        private int _activeAnim = -1;
        private double _activeFrameTime = 0.0;

        private readonly Assimp.Scene _raw;

        private int _maxBoneCount = 1;
        private bool _loop = true;
        private double _animPlaybackSpeed = 1.0;

        public SceneAnimator(Assimp.Scene raw)
        {
            _raw = raw;
            for (int i = 0; i < _raw.MeshCount; ++i)
            {
                var boneCount = _raw.Meshes[i].BoneCount;
                if (boneCount > _maxBoneCount) _maxBoneCount = boneCount;
            }
        }

        public void Update(double delta)
        {
            Cursor += delta * AnimationPlaybackSpeed;
        }

        public void UpdateAnimation()
        {
            Assimp.Animation target = _raw.Animations[ActiveAnimation];
            double animationTime = Cursor * TicksPerSecond % target.DurationInTicks;
            ProcessNode(target, (float)animationTime, _raw.RootNode, Matrix4.Identity);
        }

        public void ProcessNode(Assimp.Animation target, float animationTime, Node node, Matrix4 parentTransform)
        {
            string nodeName = node.Name;
            Matrix4 nodeTransform = Utility.FromMatrix4x4T(node.Transform);

            NodeAnimationChannel boneAnimation = FindBoneAnimation(nodeName, target);

            if (boneAnimation != null)
            {
                Quaternion interpolatedRotation = CalcInterpolatedRotation(animationTime, boneAnimation);
                Vector3 interpolatedPosition = CalcInterpolatedPosition(animationTime, boneAnimation);
                Vector3 interpolatedScale = CalcInterpolatedScale(animationTime, boneAnimation);

                nodeTransform = Matrix4.CreateFromQuaternion(interpolatedRotation) *
                                              Matrix4.CreateTranslation(interpolatedPosition) *
                                              Matrix4.CreateScale(interpolatedScale);
            }

            Matrix4 toGlobalSpace = nodeTransform * parentTransform;

            NodeState bone = FindBone(nodeName);
            if (bone != null)
                bone.Transformation = bone.Offset * toGlobalSpace;

            for (int i = 0; i < node.ChildCount; i++)
            {
                Node childNode = node.Children[i];
                ProcessNode(target, animationTime, childNode, toGlobalSpace);
            }
        }

        private NodeAnimationChannel FindBoneAnimation(string nodeName, Assimp.Animation target)
        {
            for (int i = 0; i < target.NodeAnimationChannelCount; i++)
            {
                NodeAnimationChannel nodeAnim = target.NodeAnimationChannels[i];
                if (nodeAnim.NodeName.Equals(nodeName)) return nodeAnim;
            }
            return null;
        }

        private NodeState FindBone(string nodeName)
        {
            foreach (NodeState b in _bones)
                if (b.Name.Equals(nodeName)) return b;
            return null;
        }

        private Vector3 CalcInterpolatedScale(float timeAt, NodeAnimationChannel boneAnimation)
        {
            if (boneAnimation.ScalingKeyCount == 1)
                return Utility.FromVector3Dto3(boneAnimation.ScalingKeys[0].Value);

            int index0 = FindScaleIndex(timeAt, boneAnimation);
            int index1 = index0 + 1;
            float time0 = (float)boneAnimation.ScalingKeys[index0].Time;
            float time1 = (float)boneAnimation.ScalingKeys[index1].Time;
            float deltaTime = time1 - time0;
            float percentage = (timeAt - time0) / deltaTime;

            Vector3 start = Utility.FromVector3Dto3(boneAnimation.ScalingKeys[index0].Value);
            Vector3 end = Utility.FromVector3Dto3(boneAnimation.ScalingKeys[index1].Value);
            Vector3 delta = Vector3.Subtract(end, start);
            delta = Vector3.Multiply(delta, percentage);

            return Vector3.Add(start, delta);
        }

        private int FindScaleIndex(float timeAt, NodeAnimationChannel boneAnimation)
        {
            if (boneAnimation.ScalingKeyCount > 0)
            {
                for (int i = 0; i < boneAnimation.ScalingKeyCount - 1; i++)
                {
                    if (timeAt < boneAnimation.ScalingKeys[i + 1].Time) return i;
                }
            }
            return 0;
        }

        private Quaternion CalcInterpolatedRotation(float timeAt, NodeAnimationChannel boneAnimation)
        {
            if (boneAnimation.RotationKeyCount == 1)
                return Utility.ConvertQuaternion(boneAnimation.RotationKeys[0].Value);

            int index0 = FindRotationIndex(timeAt, boneAnimation);
            int index1 = index0 + 1;
            float time0 = (float)boneAnimation.RotationKeys[index0].Time;
            float time1 = (float)boneAnimation.RotationKeys[index1].Time;
            float deltaTime = time1 - time0;
            float percentage = (timeAt - time0) / deltaTime;

            Quaternion start = Utility.ConvertQuaternion(boneAnimation.RotationKeys[index0].Value);
            Quaternion end = Utility.ConvertQuaternion(boneAnimation.RotationKeys[index1].Value);

            return Quaternion.Slerp(start, end, percentage);
        }

        private int FindRotationIndex(float timeAt, NodeAnimationChannel boneAnimation)
        {
            if (boneAnimation.RotationKeyCount > 0)
            {
                for (int i = 0; i < boneAnimation.RotationKeyCount - 1; i++)
                    if (timeAt < boneAnimation.RotationKeys[i + 1].Time) return i;
            }
            return 0;
        }

        private Vector3 CalcInterpolatedPosition(float timeAt, NodeAnimationChannel boneAnimation)
        {
            if (boneAnimation.PositionKeyCount == 1)
                return Utility.FromVector3Dto3(boneAnimation.PositionKeys[0].Value);

            int index0 = FindPositionIndex(timeAt, boneAnimation);
            int index1 = index0 + 1;
            float time0 = (float)boneAnimation.PositionKeys[index0].Time;
            float time1 = (float)boneAnimation.PositionKeys[index1].Time;
            float deltaTime = time1 - time0;
            float percentage = (timeAt - time0) / deltaTime;

            Vector3 start = Utility.FromVector3Dto3(boneAnimation.PositionKeys[index0].Value);
            Vector3 end = Utility.FromVector3Dto3(boneAnimation.PositionKeys[index1].Value);
            Vector3 delta = Vector3.Subtract(end, start);

            return Vector3.Add(start, Vector3.Multiply(delta, percentage));
        }

        private int FindPositionIndex(float timeAt, NodeAnimationChannel boneAnimation)
        {
            if (boneAnimation.PositionKeyCount > 0)
            {
                for (int i = 0; i < boneAnimation.PositionKeyCount - 1; i++)
                    if (timeAt < boneAnimation.PositionKeys[i + 1].Time) return i;
            }
            return 0;
        }

    }

    public class NodeState
    {
        public string Name { get; }
        public Matrix4 Offset { get; set; }
        public Matrix4 Transformation { get; set; }

        public NodeState(string name, Matrix4 offset)
        {
            Name = name;
            Offset = offset;
        }
    }

}
