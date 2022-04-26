using System;
using System.Net;
using System.Threading.Tasks;

namespace Entities.Connection.DataTransport
{
    public interface IDataTransport
    {
        event EventHandler<DataTransportResult> onDataReceived;

        Task<bool> Send(byte[] payload, IPEndPoint endPoint);

        void Abort();

        bool StartListening(int port);
    }

    public struct DataTransportResult
    {
        public byte[] data;
        public IPEndPoint endPoint;

        public DataTransportResult(byte[] data, IPEndPoint endPoint)
        {
            this.data = data;
            this.endPoint = endPoint;
        }
    }

    public struct DataTransportResult<T>
    {
        public T data;
        public IPEndPoint endPoint;

        public DataTransportResult(T data, IPEndPoint endPoint)
        {
            this.data = data;
            this.endPoint = endPoint;
        }
    }
}