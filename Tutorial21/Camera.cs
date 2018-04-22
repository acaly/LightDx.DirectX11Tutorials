using LightDx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial21
{
    class Camera
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, -10);
        public Vector3 Rotation { get; set; } = new Vector3(0, 0, 0);

        public Matrix4x4 GetViewMatrix()
        {
            var up = new Vector3(0, 1, 0);
            var lookAt = new Vector3(0, 0, 1);

            var rot = Matrix4x4.CreateFromYawPitchRoll(Rotation.Y * 0.015f, Rotation.X * 0.015f, Rotation.Z * 0.015f);
            up = Vector3.TransformNormal(up, rot);
            lookAt = Vector3.TransformNormal(lookAt, rot);

            lookAt += Position;

            return MatrixHelper.CreateLookAt(Position, lookAt, up);
        }
    }
}
