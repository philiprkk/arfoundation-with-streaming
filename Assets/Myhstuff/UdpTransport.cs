using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Entities.Connection.DataTransport
{
    public class UdpTransport : DataTransportThread
    {
        static private long lastTimestamp = 0;
        static private int counter = 0;
        static private int[] hits = new int[10];

        public override event EventHandler<DataTransportResult> onDataReceived;

        private UdpClient client;
        private bool broadcastListener = false;

        public UdpTransport()
        {
            client = new UdpClient();
        }
       
        #region Interface

        // Create sender and receiver client
        public void StartListeningAsBroadCaster(int port)
        {
            broadcastListener = true;
            StartListening(port);
        }

        public override bool StartListening(int port)
        {
            Abort();
            try
            {
                client = new UdpClient();

                if (broadcastListener)
                {
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.ExclusiveAddressUse = false;
                }
                
                client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                // StartListening in a new Thread after initializing client
                base.StartListeningThread();
               return true;
            }
            catch (SocketException ex)
            {
                // Persistence.Logger.LogException(ex);
                return false;
            }
        }

        public override void Abort()
        {
            base.Abort();
            try
            {
                client?.Close();
                client?.Dispose();
                client = null;
            }
            catch (Exception) { }
        }

        public override async Task<bool> Send(byte[] data, IPEndPoint endPoint)
        {
            if (client != null)
            {
                int sentBytes = await client.SendAsync(data, data.Length, endPoint);
                return sentBytes == data.Length;
            }

            return false;
        }

        #endregion
        
        protected override async Task ListenTransportInThread()
        {
            UdpReceiveResult? result = null;

            try
            {
                result = await client?.ReceiveAsync();

                if (result.Value.Buffer.Length > 0)
                {
                    hits[counter]++;
                    UpdateTimer();
                }
            }

            // Catch client socket exception
            catch (SocketException ex)
            {
                result = null;
                // Persistence.Logger.LogException(ex);
            }

            // Catch client is disposed while awaiting for result
            catch (ObjectDisposedException ex)
            {
                result = null;
            }
            catch (Exception ex)
            {
                result = null;
                // Persistence.Logger.LogException(ex);
            }

            // Raise event outside try/catch
            if (result != null)
                onDataReceived?.Invoke(this, new DataTransportResult(result.Value.Buffer, result.Value.RemoteEndPoint));
        }

        private void UpdateTimer()
        {
            long timestamp = Stopwatch.GetTimestamp();
            if ((timestamp - lastTimestamp) > Stopwatch.Frequency)
            {
                lastTimestamp = timestamp;
                counter = (counter + 1) % 10;
                hits[counter] = 0;
            }
        }

        static public int GetHitsPerSecond()
        {
            int sum = 0;
            for (int i = 0; i < 10; ++i)
            {
                if (i == counter)
                    continue;
                sum += hits[i];
            }

            return sum / 9;
        }
    }
}