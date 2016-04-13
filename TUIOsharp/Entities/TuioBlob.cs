/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TUIOsharp.Entities
{
    public class TuioBlob : TuioEntity
    {
        public float Angle { get; internal set; }
        public float Width { get; internal set; }
        public float Height { get; internal set; }
        public float Area { get; internal set; }
        public float RotationVelocity { get; internal set; }
        public float RotationAcceleration { get; internal set; }

        public TuioBlob(int id)
            : this(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
        {}

        public TuioBlob(int id, float x, float y, float angle, float width, float height, float area, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration)
            : base(id, x, y, velocityX, velocityY, acceleration)
        {
            Angle = angle;
            Width = width;
            Height = height;
            Area = area;
            RotationVelocity = rotationVelocity;
            RotationAcceleration = rotationAcceleration;
        }

        public void Update(float x, float y, float angle, float width, float height, float area, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration)
        {
            X = x;
            Y = y;
            Angle = angle;
            Width = width;
            Height = height;
            Area = area;
            VelocityX = velocityX;
            VelocityY = velocityY;
            RotationVelocity = rotationVelocity;
            Acceleration = acceleration;
            RotationAcceleration = rotationAcceleration;
        }

    }
}
