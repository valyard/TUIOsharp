/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TUIOsharp.Entities
{
    public class TuioObject : TuioEntity
    {
        public int ClassId { get; private set; }
        public float Angle { get; internal set; }
        public float RotationVelocity { get; internal set; }
        public float RotationAcceleration { get; internal set; }

        public TuioObject(int id, int classId)
            : this(id, classId, 0, 0, 0, 0, 0, 0, 0, 0)
        {}

        public TuioObject(int id, int classId, float x, float y, float angle, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration)
            : base(id, x, y, velocityX, velocityY, acceleration)
        {
            ClassId = classId;
            Angle = angle;
            RotationVelocity = rotationVelocity;
            RotationAcceleration = rotationAcceleration;
        }

        public void Update(float x, float y, float angle, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration)
        {
            X = x;
            Y = y;
            Angle = angle;
            VelocityX = velocityX;
            VelocityY = velocityY;
            RotationVelocity = rotationVelocity;
            Acceleration = acceleration;
            RotationAcceleration = rotationAcceleration;
        }

    }
}
