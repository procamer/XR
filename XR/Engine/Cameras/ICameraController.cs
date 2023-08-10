using OpenTK;

namespace XR
{

    public interface ICameraController
    {
        float Fov { get; set; }
        float Pitch { get; set; }
        float Yaw { get; set; }
        float Distance { get; set; }

        Matrix4 GetViewMatrix();
        void SetTarget(Vector3 pivot);
        void MouseMove(int x, int y);
        void Scroll(float z);
        void Pan(float x, float y);
        void MovementKey(float time);
    }
}
