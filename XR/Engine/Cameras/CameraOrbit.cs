using OpenTK;
using System;

namespace XR
{
    public class CameraOrbit : CameraBase, ICameraController
    {
        private const float MinimumCameraDistance = 0.1f;
        private float cameraDistance = 5.0f;
        private Vector3 panVector = Vector3.Zero;
        private Vector3 pivot;

        public float Distance { get { return cameraDistance; } set { cameraDistance = value; } }

        public CameraOrbit(Vector3 position)
        {
            Position = position;
        }

        public Matrix4 GetViewMatrix()
        {
            Matrix4 orientation = Matrix4.CreateFromAxisAngle(Vector3.UnitY, _yaw);
            orientation *= Matrix4.CreateFromAxisAngle(Vector3.UnitX, _pitch);
            Matrix4 viewMatrix = Matrix4.LookAt(orientation.Column2.Xyz * cameraDistance + pivot, pivot, orientation.Column1.Xyz);
            viewMatrix *= Matrix4.CreateTranslation(panVector);
            return viewMatrix;
        }

        public void MouseMove(int XDelta, int YDelta)
        {
            if (XDelta == 0 && YDelta == 0) return;
            const float sensitivity = 0.6f;
            Yaw += XDelta * sensitivity;
            Pitch += YDelta * sensitivity;
        }

        public void Scroll(float z)
        {
            const float zoomSpeed = 1.001f;
            cameraDistance *= (float)Math.Pow(zoomSpeed, -z);
            cameraDistance = Math.Max(cameraDistance, MinimumCameraDistance);
        }

        public void SetTarget(Vector3 pivot)
        {
            this.pivot = pivot;
        }

        public void Pan(float x, float y)
        {
            const float panSpeed = 0.004f;
            panVector.X += x * panSpeed;
            panVector.Y += -y * panSpeed;
        }

    }
}
