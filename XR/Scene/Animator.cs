using Assimp;
using OpenTK;
using System.Collections.Generic;

namespace XR
{
    public class BoneT
    {
        public string Name { get; }
        public Matrix4 Offset { get; set; }
        public Matrix4 Transformation { get; set; }

        public BoneT(string name, Matrix4 offset)
        {
            Name = name;
            Offset = offset;
        }
    }

    public class Animator
    {
        public List<Animation> animations; 
        internal List<BoneT> bones; 
        internal Node rootNode; 

        public Animator(List<Animation> animations, Node rootNode)
        {
            this.animations = animations;
            this.rootNode = rootNode;
        }

        public void UpdateAnimation(float time, int animationIndex)
        {
            if (animationIndex >= 0 && animationIndex < animations.Count)
            {
                Animation target = animations[animationIndex];
                float tickPerSecond = target.TicksPerSecond != 0 ? (float)target.TicksPerSecond : 60.0f;
                float ticks = time * tickPerSecond;
                float animationTime = ticks % (float)target.DurationInTicks;
                ProcessNode(target, animationTime, rootNode, Matrix4.Identity);
            }
        }

        public void ProcessNode(Animation target, float animationTime, Node node, Matrix4 parentTransform)
        {
            string nodeName = node.Name;
            Matrix4 nodeTransform = Utility.FromMatrix4x4T(node.Transform);

            NodeAnimationChannel boneAnimation = FindBoneAnimation(target, nodeName);

            if (boneAnimation != null)
            {
                Vector3 interpolatedScale = CalcInterpolateScale(animationTime, boneAnimation);
                Matrix4 scaleMatrix = Matrix4.CreateScale(interpolatedScale);

                OpenTK.Quaternion interpolatedRotation = CalcInterpolatedRotation(animationTime, boneAnimation);
                Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(interpolatedRotation);

                Vector3 interpolatedPosition = CalcInterpolatedPosition(animationTime, boneAnimation);
                Matrix4 translationMatrix = Matrix4.CreateTranslation(interpolatedPosition);

                nodeTransform = Matrix4.Mult(rotationMatrix, translationMatrix);
                nodeTransform = Matrix4.Mult(scaleMatrix, nodeTransform);
            }

            Matrix4 toGlobalSpace = Matrix4.Mult(nodeTransform, parentTransform);

            BoneT bone = FindBone(nodeName);
            if (bone != null)
                bone.Transformation = Matrix4.Mult(bone.Offset, toGlobalSpace);

            for (int i = 0; i < node.ChildCount; i++)
            {
                Node childNode = node.Children[i];
                ProcessNode(target, animationTime, childNode, toGlobalSpace);
            }
        }

        private NodeAnimationChannel FindBoneAnimation(Animation target, string nodeName)
        {
            for (int i = 0; i < target.NodeAnimationChannelCount; i++)
            {
                NodeAnimationChannel nodeAnim = target.NodeAnimationChannels[i];
                if (nodeAnim.NodeName.Equals(nodeName)) return nodeAnim;
            }
            return null;
        }

        private Vector3 CalcInterpolateScale(float timeAt, NodeAnimationChannel boneAnimation)
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

        private OpenTK.Quaternion CalcInterpolatedRotation(float timeAt, NodeAnimationChannel boneAnimation)
        {
            if (boneAnimation.RotationKeyCount == 1)
                return Utility.ConvertQuaternion(boneAnimation.RotationKeys[0].Value);

            int index0 = FindRotationIndex(timeAt, boneAnimation);
            int index1 = index0 + 1;
            float time0 = (float)boneAnimation.RotationKeys[index0].Time;
            float time1 = (float)boneAnimation.RotationKeys[index1].Time;
            float deltaTime = time1 - time0;
            float percentage = (timeAt - time0) / deltaTime;

            OpenTK.Quaternion start = Utility.ConvertQuaternion(boneAnimation.RotationKeys[index0].Value);
            OpenTK.Quaternion end = Utility.ConvertQuaternion(boneAnimation.RotationKeys[index1].Value);

            return OpenTK.Quaternion.Slerp(start, end, percentage);
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

        private BoneT FindBone(string nodeName)
        {
            foreach (BoneT b in bones)
                if (b.Name.Equals(nodeName)) return b;
            return null;
        }

    }
}
