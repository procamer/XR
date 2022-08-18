using OpenTK;
using OpenTK.Input;
using System;

namespace XR
{
    public class CameraBase
    {
        // Those vectors are directions pointing outwards from the camera to define how it rotated
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        public float _pitch = 0f;
        // Rotation around the Y axis (radians)
        public float _yaw = 0f; //-MathHelper.PiOver2; // Without this you would be started rotated 90 degrees right
        // The field of view of the camera (radians)
        private float _fov = MathHelper.PiOver4;

        // The position of the camera
        public Vector3 Position { get; set; }
        // This is simply the aspect ratio of the viewport, used for the projection matrix
        //public float AspectRatio { get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                float angle = MathHelper.Clamp(value, -90, 90);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value) % MathHelper.TwoPi;
                if (_yaw < 0) _yaw += MathHelper.TwoPi;
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        private void UpdateVectors()
        {
            // First the front matrix is calculated using some basic trigonometry
            _front.X = (float)Math.Cos(_pitch) * (float)Math.Cos(_yaw);
            _front.Y = (float)Math.Sin(_pitch);
            _front.Z = (float)Math.Cos(_pitch) * (float)Math.Sin(_yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product
            // Note that we are calculating the right from the global up, this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }

        public void MovementKey(float time)
        {
            KeyboardState keyboard = Keyboard.GetState();
            const float cameraSpeed = 1.5f;
            if (keyboard.IsKeyDown(Key.W))
                Position += Front * cameraSpeed * time; // Forward
            if (keyboard.IsKeyDown(Key.S))
                Position -= Front * cameraSpeed * time; // Backwards
            if (keyboard.IsKeyDown(Key.A))
                Position -= Right * cameraSpeed * time; // Left
            if (keyboard.IsKeyDown(Key.D))
                Position += Right * cameraSpeed * time; // Right
            if (keyboard.IsKeyDown(Key.Space))
                Position += Up * cameraSpeed * time; // Up
            if (keyboard.IsKeyDown(Key.LShift))
                Position -= Up * cameraSpeed * time; // Down
        }

    }
}
