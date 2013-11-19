/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using OSCsharp.Data;
using OSCsharp.Net;
using OSCsharp.Utils;

namespace TUIOsharp
{
    public class TuioServer
    {
        #region Private vars

        private UDPReceiver udpReceiver;

        private Dictionary<int, TuioCursor> cursors = new Dictionary<int, TuioCursor>();

        private List<TuioCursor> updatedCursors = new List<TuioCursor>();
        private List<int> addedCursors = new List<int>();
        private List<int> removedCursors = new List<int>();

        private float movementThreshold = 0;
        private float movementThresholdSq = 0;

        #endregion

        #region Public properties

        public float MovementThreshold
        {
            get { return movementThreshold; }
            set
            {
                movementThreshold = value;
                movementThresholdSq = value*value;
            }
        }

        public int Port { get; private set; }
        public int FrameNumber { get; private set; }

        #endregion

        #region Events

        public event EventHandler<TuioCursorEventArgs> CursorAdded;
        public event EventHandler<TuioCursorEventArgs> CursorUpdated;
        public event EventHandler<TuioCursorEventArgs> CursorRemoved;
        public event EventHandler<ExceptionEventArgs> ErrorOccured;

        #endregion

        #region Constructors

        public TuioServer() : this(3333)
        {}

        public TuioServer(int port)
        {
            Port = port;

            MovementThreshold = 0;

            udpReceiver = new UDPReceiver(Port, false);
            udpReceiver.MessageReceived += handlerOscMessageReceived;
            udpReceiver.ErrorOccured += handlerOscErrorOccured;
        }

        #endregion

        #region Public methods

        public void Connect()
        {
            if (!udpReceiver.IsRunning) udpReceiver.Start();
        }

        public void Disconnect()
        {
            if (udpReceiver.IsRunning) udpReceiver.Stop();
        }

        #endregion

        #region Private functions

        private void parseOscMessage(OscMessage message)
        {
            switch (message.Address)
            {
                case "/tuio/2Dcur":
                    if (message.Data.Count == 0) return;
                    var command = message.Data[0].ToString();
                    switch (command)
                    {
                        case "set":
                            if (message.Data.Count < 4) return;
                            var id = (int)message.Data[1];
                            var xPos = (float)message.Data[2];
                            var yPos = (float)message.Data[3];
                            TuioCursor cursor;
                            if (!cursors.TryGetValue(id, out cursor))
                            {
                                cursor = new TuioCursor(id);
                            }
                            var deltaX = cursor.X - xPos;
                            var deltaY = cursor.Y - yPos;
                            if (deltaX*deltaX + deltaY*deltaY >= movementThresholdSq)
                            {
                                cursor.Update(xPos, yPos);
                                updatedCursors.Add(cursor);
                            }
                            break;
                        case "alive":
                            var aliveCursors = new List<int>();
                            for (var i = 1; i < message.Data.Count; i++)
                            {
                                aliveCursors.Add((int)message.Data[i]);
                            }
                            foreach (KeyValuePair<int, TuioCursor> value in cursors)
                            {
                                if (!aliveCursors.Contains(value.Key))
                                {
                                    removedCursors.Add(value.Key);
                                }
                                aliveCursors.Remove(value.Key);
                            }
                            addedCursors.AddRange(aliveCursors);
                            break;
                        case "fseq":
                            if (message.Data.Count < 2) return;
                            FrameNumber = (int)message.Data[1];
                            foreach (var updatedCursor in updatedCursors)
                            {
                                if (addedCursors.Contains(updatedCursor.Id) && !cursors.ContainsKey(updatedCursor.Id))
                                {
                                    cursors.Add(updatedCursor.Id, updatedCursor);
                                    if (CursorAdded != null) CursorAdded(this, new TuioCursorEventArgs(updatedCursor));
                                } else
                                {
                                    if (CursorUpdated != null) CursorUpdated(this, new TuioCursorEventArgs(updatedCursor));
                                }
                            }
                            foreach (var cursorId in removedCursors)
                            {
                                cursor = cursors[cursorId];
                                cursors.Remove(cursorId);
                                if (CursorRemoved != null) CursorRemoved(this, new TuioCursorEventArgs(cursor));
                            }

                            addedCursors.Clear();
                            removedCursors.Clear();
                            updatedCursors.Clear();
                            break;
                    }
                    break;
            }
        }

        #endregion

        #region Event handlers

        private void handlerOscErrorOccured(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            if (ErrorOccured != null) ErrorOccured(this, exceptionEventArgs);
        }

        private void handlerOscMessageReceived(object sender, OscMessageReceivedEventArgs oscMessageReceivedEventArgs)
        {
            parseOscMessage(oscMessageReceivedEventArgs.Message);
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