// using Entities.Persistence;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Entities.Connection.DataTransport
{
    /// <summary>
    /// Base DataTransport class for initializing a new thread to listen for data.
    /// </summary>
    public abstract class DataTransportThread : IDataTransport
    {
        public static bool ForceSingleThread = false;

        public abstract event EventHandler<DataTransportResult> onDataReceived;

        public bool isListening { get; private set; }

        protected Thread listenThread;

        protected async void StartListeningThread()
        {
            if (isListening)
            {
                // Logger.LogWarning("Requested DataTransportThread to StartListening, but is already listening.");

                return;
            }

            if (ForceSingleThread)
            {
                await StartLoopListen();

                return;
            }

            listenThread = new Thread(async () =>
            {
                try
                {
                    await StartLoopListen();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    isListening = false;
                }
            });
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private async Task StartLoopListen()
        {
            isListening = true;

            while (isListening)
            {
                await ListenTransportInThread();
            }
        }

        #region Inheritance

        protected abstract Task ListenTransportInThread();

        #endregion

        #region Interface

        public abstract bool StartListening(int port);

        public abstract Task<bool> Send(byte[] payload, IPEndPoint endPoint);

        public virtual void Abort()
        {
            isListening = false;

            // https://docs.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/5.0/thread-abort-obsolete
            //listenThread?.Abort();
            listenThread = null;
        }

        #endregion

    }
}