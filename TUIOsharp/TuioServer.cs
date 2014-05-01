/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using OSCsharp.Data;
using OSCsharp.Net;
using OSCsharp.Utils;
using TUIOsharp.Entities;

namespace TUIOsharp
{
    public class TuioServer
    {
        #region Private vars

        private UDPReceiver udpReceiver;

        private Dictionary<int, TuioCursor> cursors = new Dictionary<int, TuioCursor>();
        private Dictionary<int, TuioBlob> blobs = new Dictionary<int, TuioBlob>();
        private Dictionary<int, TuioObject> objects = new Dictionary<int, TuioObject>();

        private List<TuioCursor> updatedCursors = new List<TuioCursor>();
        private List<int> addedCursors = new List<int>();
        private List<int> removedCursors = new List<int>();
        private List<TuioBlob> updatedBlobs = new List<TuioBlob>();
        private List<int> addedBlobs = new List<int>();
        private List<int> removedBlobs = new List<int>();
        private List<TuioObject> updatedObjects = new List<TuioObject>();
        private List<int> addedObjects = new List<int>();
        private List<int> removedObjects = new List<int>();

        #endregion

        #region Public properties

        public int Port { get; private set; }
        public int FrameNumber { get; private set; }

        #endregion

        #region Events

        public event EventHandler<TuioCursorEventArgs> CursorAdded;
        public event EventHandler<TuioCursorEventArgs> CursorUpdated;
        public event EventHandler<TuioCursorEventArgs> CursorRemoved;
        public event EventHandler<TuioObjectEventArgs> ObjectAdded;
        public event EventHandler<TuioObjectEventArgs> ObjectUpdated;
        public event EventHandler<TuioObjectEventArgs> ObjectRemoved;
        public event EventHandler<TuioBlobEventArgs> BlobAdded;
        public event EventHandler<TuioBlobEventArgs> BlobUpdated;
        public event EventHandler<TuioBlobEventArgs> BlobRemoved;
        public event EventHandler<ExceptionEventArgs> ErrorOccured;

        #endregion

        #region Constructors

        public TuioServer() : this(3333)
        {}

        public TuioServer(int port)
        {
            Port = port;

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
            if (message.Data.Count == 0) return;

            var command = message.Data[0].ToString();
            switch (message.Address)
            {
                case "/tuio/2Dcur":
                {
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
                case "/tuio/2Dobj":
                {
                    switch (command)
                    {
                        case "set":
                            if (message.Data.Count < 11) return;

                            var id = (int)message.Data[1];
                            var classId = (int)message.Data[2];
                            var xPos = (float)message.Data[3];
                            var yPos = (float)message.Data[4];
                            var angle = (float)message.Data[5];
                            var velocityX = (float)message.Data[6];
                            var velocityY = (float)message.Data[7];
                            var rotationVelocity = (float)message.Data[8];
                            var acceleration = (float)message.Data[9];
                            var rotationAcceleration = (float)message.Data[10];

                            TuioObject obj;
                            if (!objects.TryGetValue(id, out obj)) obj = new TuioObject(id, classId);
                            obj.Update(xPos, yPos, angle, velocityX, velocityY, rotationVelocity, acceleration, rotationAcceleration);
                            updatedObjects.Add(obj);
                            break;
                        case "alive":
                            var total = message.Data.Count;
                            for (var i = 1; i < total; i++)
                            {
                                addedObjects.Add((int)message.Data[i]);
                            }
                            foreach (KeyValuePair<int, TuioCursor> value in cursors)
                            {
                                if (!addedObjects.Contains(value.Key))
                                {
                                    removedObjects.Add(value.Key);
                                }
                                addedObjects.Remove(value.Key);
                            }
                            break;
                        case "fseq":
                            if (message.Data.Count < 2) return;
                            FrameNumber = (int)message.Data[1];
                            foreach (var updatedObject in updatedObjects)
                            {
                                if (addedObjects.Contains(updatedObject.Id) && !objects.ContainsKey(updatedObject.Id))
                                {
                                    objects.Add(updatedObject.Id, updatedObject);
                                    if (ObjectAdded != null) ObjectAdded(this, new TuioObjectEventArgs(updatedObject));
                                } else
                                {
                                    if (ObjectUpdated != null) ObjectUpdated(this, new TuioObjectEventArgs(updatedObject));
                                }
                            }
                            foreach (var objectId in removedObjects)
                            {
                                obj = objects[objectId];
                                objects.Remove(objectId);
                                if (ObjectRemoved != null) ObjectRemoved(this, new TuioObjectEventArgs(obj));
                            }

                            addedObjects.Clear();
                            removedObjects.Clear();
                            updatedObjects.Clear();
                            break;
                    }
                    break;
                }
                case "/tuio/2Dblb":
                {
                    switch (command)
                    {
                        case "set":
                            if (message.Data.Count < 13) return;

                            var id = (int)message.Data[1];
                            var xPos = (float)message.Data[2];
                            var yPos = (float)message.Data[3];
                            var angle = (float)message.Data[4];
                            var width = (float)message.Data[5];
                            var height = (float)message.Data[6];
                            var area = (float)message.Data[7];
                            var velocityX = (float)message.Data[8];
                            var velocityY = (float)message.Data[9];
                            var rotationVelocity = (float)message.Data[10];
                            var acceleration = (float)message.Data[11];
                            var rotationAcceleration = (float)message.Data[12];

                            TuioBlob blob;
                            if (!blobs.TryGetValue(id, out blob)) blob = new TuioBlob(id);
                            blob.Update(xPos, yPos, angle, width, height, area, velocityX, velocityY, rotationVelocity, acceleration, rotationAcceleration);
                            updatedBlobs.Add(blob);
                            break;
                        case "alive":
                            var total = message.Data.Count;
                            for (var i = 1; i < total; i++)
                            {
                                addedBlobs.Add((int)message.Data[i]);
                            }
                            foreach (KeyValuePair<int, TuioBlob> value in blobs)
                            {
                                if (!addedBlobs.Contains(value.Key))
                                {
                                    removedBlobs.Add(value.Key);
                                }
                                addedBlobs.Remove(value.Key);
                            }
                            break;
                        case "fseq":
                            if (message.Data.Count < 2) return;
                            FrameNumber = (int)message.Data[1];
                            foreach (var updatedBlob in updatedBlobs)
                            {
                                if (addedBlobs.Contains(updatedBlob.Id) && !blobs.ContainsKey(updatedBlob.Id))
                                {
                                    blobs.Add(updatedBlob.Id, updatedBlob);
                                    if (BlobAdded != null) BlobAdded(this, new TuioBlobEventArgs(updatedBlob));
                                } else
                                {
                                    if (BlobUpdated != null) BlobUpdated(this, new TuioBlobEventArgs(updatedBlob));
                                }
                            }
                            foreach (var blobId in removedBlobs)
                            {
                                blob = blobs[blobId];
                                blobs.Remove(blobId);
                                if (BlobRemoved != null) BlobRemoved(this, new TuioBlobEventArgs(blob));
                            }

                            addedBlobs.Clear();
                            removedBlobs.Clear();
                            updatedBlobs.Clear();
                            break;
                    }
                    break;
                }
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

    public class TuioObjectEventArgs : EventArgs
    {
        public TuioObject Object;

        public TuioObjectEventArgs(TuioObject obj)
        {
            Object = obj;
        }
    }

    public class TuioBlobEventArgs : EventArgs
    {
        public TuioBlob Blob;

        public TuioBlobEventArgs(TuioBlob blob)
        {
            Blob = blob;
        }
    }
}