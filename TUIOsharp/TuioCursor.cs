/*
 * @author Valentin Simonov / http://va.lent.in/
 */

namespace TUIOsharp
{
    public class TuioCursor : TuioEntity
    {
        public TuioCursor(int id)
            : this(id, 0, 0)
        {}

        public TuioCursor(int id, float x, float y) : base(id, x, y)
        {}
    }
}