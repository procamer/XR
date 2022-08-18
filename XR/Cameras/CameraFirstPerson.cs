using OpenTK;

namespace XR
{
    public class CameraFPS : CameraBase, ICameraController
    {
        public float Distance { get; set; }

        public CameraFPS(Vector3 position)
        {
            Position = position;
        }

        public Matrix4 GetViewMatrix()
        {
            Matrix4 viewMatrix;
            viewMatrix = Matrix4.LookAt(Position, Position + Front, Up);
            return viewMatrix;
        }

        public void MouseMove(int xDelta, int yDelta)
        {
            if (xDelta == 0 && yDelta == 0) return;
            const float sensitivity = 0.2f;
            Yaw += xDelta * sensitivity;
            Pitch -= yDelta * sensitivity;
        }

        public void Scroll(float z)
        {
            const float zoomSpeed = 0.02f;
            Fov -= z * zoomSpeed;
        }

        public void SetTarget(Vector3 pivot)
        {
        }

        public void Pan(float x, float y)
        {
        }

    }
}
