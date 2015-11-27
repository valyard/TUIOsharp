/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using System.Collections.Generic;
using OSCsharp.Data;
using OSCsharp.Net;
using OSCsharp.Utils;
using TUIOsharp.DataProcessors;

namespace TUIOsharp
{
    public class TuioServer
    {

        #region Events

        public event EventHandler<ExceptionEventArgs> ErrorOccured;

        #endregion

        #region Public properties

        public int Port { get; private set; }

        #endregion

        #region Private vars

        private UDPReceiver udpReceiver;
        private List<IDataProcessor> processors = new List<IDataProcessor>(); 

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

        public void AddDataProcessor(IDataProcessor processor)
        {
            if (processors.Contains(processor)) return;

            processors.Add(processor);
        }

        public void RemoveDataProcessor(IDataProcessor processor)
        {
            processors.Remove(processor);
        }

        public void RemoveAllDataProcessors()
        {
            processors.Clear();
        }

        #endregion

        #region Private functions

        private void processMessage(OscMessage message)
        {
            if (message.Data.Count == 0) return;

            var count = processors.Count;
            for (var i = 0; i < count; i++)
            {
                processors[i].ProcessMessage(message);
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
            processMessage(oscMessageReceivedEventArgs.Message);
        }

        #endregion
    }
    
}