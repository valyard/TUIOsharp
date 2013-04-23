/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TUIOsharp
{
    public abstract class TuioEntity
    {
        public int Id { get; internal set; }
        public float X { get; set; }
        public float Y { get; set; }

        protected TuioEntity(int id) : this(id, 0, 0)
        {}

        protected TuioEntity(int id, float x, float y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public void Update(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}