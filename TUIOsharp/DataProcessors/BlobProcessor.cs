/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using OSCsharp.Data;
using TUIOsharp.Entities;

namespace TUIOsharp.DataProcessors
{
    public class BlobProcessor : IDataProcessor
    {

        #region Events

        public event EventHandler<TuioBlobEventArgs> BlobAdded;
        public event EventHandler<TuioBlobEventArgs> BlobUpdated;
        public event EventHandler<TuioBlobEventArgs> BlobRemoved;

        #endregion

        #region Public properties

        public int FrameNumber { get; private set; }

        #endregion

        #region Private vars

        private Dictionary<int, TuioBlob> blobs = new Dictionary<int, TuioBlob>();

        private List<TuioBlob> updatedBlobs = new List<TuioBlob>();
        private List<int> addedBlobs = new List<int>();
        private List<int> removedBlobs = new List<int>();

        #endregion

        #region Public methods

        public void ProcessMessage(OscMessage message)
        {
            if (message.Address != "/tuio/2Dblb") return;

            var command = message.Data[0].ToString();
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
                    var count = updatedBlobs.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var updatedBlob = updatedBlobs[i];
                        if (addedBlobs.Contains(updatedBlob.Id) && !blobs.ContainsKey(updatedBlob.Id))
                        {
                            blobs.Add(updatedBlob.Id, updatedBlob);
                            if (BlobAdded != null) BlobAdded(this, new TuioBlobEventArgs(updatedBlob));
                        }
                        else
                        {
                            if (BlobUpdated != null) BlobUpdated(this, new TuioBlobEventArgs(updatedBlob));
                        }
                    }
                    count = removedBlobs.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var blobId = removedBlobs[i];
                        blob = blobs[blobId];
                        blobs.Remove(blobId);
                        if (BlobRemoved != null) BlobRemoved(this, new TuioBlobEventArgs(blob));
                    }

                    addedBlobs.Clear();
                    removedBlobs.Clear();
                    updatedBlobs.Clear();
                    break;
            }
        }

        #endregion
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
