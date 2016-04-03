/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TUIOsharp.Entities
{
    public class TuioCursor : TuioEntity
    {
        public TuioCursor(int id) : this(id, 0, 0, 0, 0, 0)
        {}

        public TuioCursor(int id, float x, float y, float velocityX, float velocityY, float acceleration)
            : base(id, x, y, velocityX, velocityY, acceleration)
        {}

        public void Update(float x, float y, float velocityX, float velocityY, float acceleration)
        {
            X = x;
            Y = y;
            VelocityX = velocityX;
            VelocityY = velocityY;
            Acceleration = acceleration;
        }

    }
}