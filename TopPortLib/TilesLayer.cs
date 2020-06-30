using Crow.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    internal class TilesLayer : ITilesLayer<byte[], byte[]>
    {
        private ITopPort _topPort;
        public event Action<byte[]> OnReceiveData;

        public TilesLayer(ITopPort topPort)
        {
            _topPort = topPort;
            _topPort.OnReceiveParsedData += async data => OnReceiveData?.Invoke(data);
        }

        public async Task SendAsync(byte[] data)
        {
            await _topPort.SendAsync(data);
        }
    }
}
