/**
 * Copyright (C) 2012 Interactive Lab
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using OSCsharp;
using OSCsharp.Utils;

namespace TuioSharp {
    public class TuioServer
    {
        #region Private vars
        private readonly OscServer oscServer;

        private readonly Dictionary<int, TuioCursor> cursors = new Dictionary<int, TuioCursor>();

        private readonly List<TuioCursor> updatedCursors = new List<TuioCursor>();
        private readonly List<int> addedCursors = new List<int>();
        private readonly List<int> removedCursors = new List<int>();
        #endregion

        #region Public properties
        public int Port { get; private set; }
        public int FrameNumber { get; private set; }
        #endregion

        #region Events
        public event EventHandler<TuioCursorEventArgs> CursorAdded;
        public event EventHandler<TuioCursorEventArgs> CursorUpdated;
        public event EventHandler<TuioCursorEventArgs> CursorRemoved;
        #endregion

        #region Constructors
        public TuioServer() : this(3333) {}

        public TuioServer(int port) {
            Port = port;

            oscServer = new OscServer(TransportType.Udp, IPAddress.Any, Port) {FilterRegisteredMethods = false, ConsumeParsingExceptions = false};
            oscServer.BundleReceived += OscServerOnBundleReceived;
            oscServer.MessageReceived += OscServerOnMessageReceived;
            oscServer.ReceiveErrored += OscServerOnReceiveErrored;
        }
        #endregion

        #region Public methods
        public void Connect() {
            if (!oscServer.IsRunning) oscServer.Start();
        }

        public void Disconnect() {
            if (oscServer.IsRunning) oscServer.Stop();
        }
        #endregion

        #region Private functions
        private void parseOscMessage(OscMessage message) {
            switch (message.Address) {
                case "/tuio/2Dcur":
                    if (message.Data.Count == 0) return;
                    var command = message.Data[0].ToString();
                    switch (command) {
                        case "set":
                            if (message.Data.Count < 4) return;
                            var id = (int) message.Data[1];
                            var xPos = (float) message.Data[2];
                            var yPos = (float) message.Data[3];
                            TuioCursor cursor;
                            if (!cursors.TryGetValue(id, out cursor)) {
                                cursor = new TuioCursor(id);
                            }
                            if (xPos != cursor.X || yPos != cursor.Y) {
                                cursor.Update(xPos, yPos);
                                updatedCursors.Add(cursor);
                            }
                            break;
                        case "alive":
                            var aliveCursors = new List<int>();
                            for (var i = 1; i < message.Data.Count; i++) {
                                aliveCursors.Add((int) message.Data[i]);
                            }
                            foreach (KeyValuePair<int, TuioCursor> value in cursors) {
                                if (!aliveCursors.Contains(value.Key)) {
                                    removedCursors.Add(value.Key);
                                }
                                aliveCursors.Remove(value.Key);
                            }
                            addedCursors.AddRange(aliveCursors);
                            break;
                        case "fseq":
                            if (message.Data.Count < 2) return;
                            FrameNumber = (int) message.Data[1];
                            foreach (var updatedCursor in updatedCursors) {
                                if (addedCursors.Contains(updatedCursor.Id) && !cursors.ContainsKey(updatedCursor.Id)) {
                                    cursors.Add(updatedCursor.Id, updatedCursor);
                                    if (CursorAdded != null) CursorAdded(this, new TuioCursorEventArgs(updatedCursor));
                                } else {
                                    if (CursorUpdated != null) CursorUpdated(this, new TuioCursorEventArgs(updatedCursor));
                                }
                            }
                            foreach (var cursorId in removedCursors) {
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
        private void OscServerOnReceiveErrored(object sender, ExceptionEventArgs exceptionEventArgs) {}

        private void OscServerOnBundleReceived(object sender, OscBundleReceivedEventArgs oscBundleReceivedEventArgs) {}

        private void OscServerOnMessageReceived(object sender, OscMessageReceivedEventArgs oscMessageReceivedEventArgs) {
            parseOscMessage(oscMessageReceivedEventArgs.Message);
        }
        #endregion
    }

    public class TuioCursorEventArgs : EventArgs {
        public TuioCursor Cursor;

        public TuioCursorEventArgs(TuioCursor cursor) {
            Cursor = cursor;
        }
    }
}