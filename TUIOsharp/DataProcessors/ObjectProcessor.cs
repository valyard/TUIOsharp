/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using OSCsharp.Data;
using TUIOsharp.Entities;

namespace TUIOsharp.DataProcessors
{
    public class ObjectProcessor : IDataProcessor
    {

        #region Events

        public event EventHandler<TuioObjectEventArgs> ObjectAdded;
        public event EventHandler<TuioObjectEventArgs> ObjectUpdated;
        public event EventHandler<TuioObjectEventArgs> ObjectRemoved;

        #endregion

        #region Public properties

        public int FrameNumber { get; private set; }

        #endregion

        #region Private vars

        private Dictionary<int, TuioObject> objects = new Dictionary<int, TuioObject>();

        private List<TuioObject> updatedObjects = new List<TuioObject>();
        private List<int> addedObjects = new List<int>();
        private List<int> removedObjects = new List<int>();

        #endregion

        #region Public methods

        public void ProcessMessage(OscMessage message)
        {
            if (message.Address != "/tuio/2Dobj") return;

            var command = message.Data[0].ToString();
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
                    foreach (KeyValuePair<int, TuioObject> value in objects)
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
                    var count = updatedObjects.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var updatedObject = updatedObjects[i];
                        if (addedObjects.Contains(updatedObject.Id) && !objects.ContainsKey(updatedObject.Id))
                        {
                            objects.Add(updatedObject.Id, updatedObject);
                            if (ObjectAdded != null) ObjectAdded(this, new TuioObjectEventArgs(updatedObject));
                        }
                        else
                        {
                            if (ObjectUpdated != null) ObjectUpdated(this, new TuioObjectEventArgs(updatedObject));
                        }
                    }
                    count = removedObjects.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var objectId = removedObjects[i];
                        obj = objects[objectId];
                        objects.Remove(objectId);
                        if (ObjectRemoved != null) ObjectRemoved(this, new TuioObjectEventArgs(obj));
                    }

                    addedObjects.Clear();
                    removedObjects.Clear();
                    updatedObjects.Clear();
                    break;
            }
        }

        #endregion

    }

    public class TuioObjectEventArgs : EventArgs
    {
        public TuioObject Object;

        public TuioObjectEventArgs(TuioObject obj)
        {
            Object = obj;
        }
    }
}
