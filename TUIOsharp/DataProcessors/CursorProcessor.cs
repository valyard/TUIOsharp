/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using OSCsharp.Data;
using TUIOsharp.Entities;

namespace TUIOsharp.DataProcessors
{
    public class CursorProcessor : IDataProcessor
    {

        #region Events

        public event EventHandler<TuioCursorEventArgs> CursorAdded;
        public event EventHandler<TuioCursorEventArgs> CursorUpdated;
        public event EventHandler<TuioCursorEventArgs> CursorRemoved;

        #endregion

        #region Public properties

        public int FrameNumber { get; private set; }

        #endregion

        #region Private vars

        private Dictionary<int, TuioCursor> cursors = new Dictionary<int, TuioCursor>();

        private List<TuioCursor> updatedCursors = new List<TuioCursor>();
        private List<int> addedCursors = new List<int>();
        private List<int> removedCursors = new List<int>();

        #endregion

        #region Public methods

        public void ProcessMessage(OscMessage message)
        {
            if (message.Address != "/tuio/2Dcur") return;

            var command = message.Data[0].ToString();
            switch (command)
            {
                case "set":
                    if (message.Data.Count < 7) return;

                    var id = (int)message.Data[1];
                    var xPos = (float)message.Data[2];
                    var yPos = (float)message.Data[3];
                    var velocityX = (float)message.Data[4];
                    var velocityY = (float)message.Data[5];
                    var acceleration = (float)message.Data[6];

                    TuioCursor cursor;
                    if (!cursors.TryGetValue(id, out cursor)) cursor = new TuioCursor(id);
                    cursor.Update(xPos, yPos, velocityX, velocityY, acceleration);
                    updatedCursors.Add(cursor);
                    break;
                case "alive":
                    var total = message.Data.Count;
                    for (var i = 1; i < total; i++)
                    {
                        addedCursors.Add((int)message.Data[i]);
                    }
                    foreach (KeyValuePair<int, TuioCursor> value in cursors)
                    {
                        if (!addedCursors.Contains(value.Key))
                        {
                            removedCursors.Add(value.Key);
                        }
                        addedCursors.Remove(value.Key);
                    }
                    break;
                case "fseq":
                    if (message.Data.Count < 2) return;
                    FrameNumber = (int)message.Data[1];
                    var count = updatedCursors.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var updatedCursor = updatedCursors[i];
                        if (addedCursors.Contains(updatedCursor.Id) && !cursors.ContainsKey(updatedCursor.Id))
                        {
                            cursors.Add(updatedCursor.Id, updatedCursor);
                            if (CursorAdded != null) CursorAdded(this, new TuioCursorEventArgs(updatedCursor));
                        }
                        else
                        {
                            if (CursorUpdated != null) CursorUpdated(this, new TuioCursorEventArgs(updatedCursor));
                        }
                    }
                    count = removedCursors.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var cursorId = removedCursors[i];
                        cursor = cursors[cursorId];
                        cursors.Remove(cursorId);
                        if (CursorRemoved != null) CursorRemoved(this, new TuioCursorEventArgs(cursor));
                    }

                    addedCursors.Clear();
                    removedCursors.Clear();
                    updatedCursors.Clear();
                    break;
            }
        }

        #endregion

    }

    public class TuioCursorEventArgs : EventArgs
    {
        public TuioCursor Cursor;

        public TuioCursorEventArgs(TuioCursor cursor)
        {
            Cursor = cursor;
        }
    }
}
