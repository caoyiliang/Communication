/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：TopPort.cs
********************************************************************/

using Communication.Bus;
using Communication.Interfaces;
using TopPortLib.Interfaces;
using Parser;
using Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Communication.Exceptions;

namespace TopPortLib
{
    public class TopPort : ITopPort, IDisposable
    {
        private IBusPort _port;
        private IParser _parser;

        public IPhysicalPort PhysicalPort { get => _port.PhysicalPort; set => _port.PhysicalPort = value; }

        public event ReceiveParsedDataEventHandler OnReceiveParsedData;

        public TopPort(IPhysicalPort physicalPort, IParser parser)
        {
            this._parser = parser;
            parser.OnReceiveParsedData += data => this.OnReceiveParsedData?.Invoke(data);

            this._port = new BusPort(physicalPort);

            _port.OnReceiveOriginalData += parser.ReceiveOriginalDataAsync;
        }

        public async Task CloseAsync()
        {
            await _port.CloseAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ConnectFailedException"></exception>
        /// <returns></returns>
        public async Task OpenAsync()
        {
            await _port.OpenAsync();
        }

        public async Task SendAsync(byte[] data, int timeInterval = 0)
        {
            await _port.SendAsync(data, timeInterval);
        }

        public void Dispose()
        {
            if (_parser is IDisposable needDisposingParser)
            {
                needDisposingParser.Dispose();
            }
            var task = this.CloseAsync();
            task.ConfigureAwait(false);
            task.Wait();
        }
    }
}
