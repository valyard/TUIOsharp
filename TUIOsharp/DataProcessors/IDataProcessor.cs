/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using OSCsharp.Data;

namespace TUIOsharp.DataProcessors
{
    public interface IDataProcessor
    {
        void ProcessMessage(OscMessage message);
    }
}
