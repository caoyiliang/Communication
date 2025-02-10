using Communication;
using Communication.Interfaces;
using Parser.Interfaces;
using System.Collections.Concurrent;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    /// <summary>
    /// 多对对通讯口
    /// </summary>
    public class TopPort_M2M : ITopPort_M2M
    {
        private readonly ConcurrentDictionary<Guid, IParser> _dicParsers = new();

        /// <inheritdoc/>
        public IPhysicalPort_M2M PhysicalPort { get; }

        /// <inheritdoc/>
        public event SentDataToClientEventHandler<byte[]>? OnSentData;
        /// <inheritdoc/>
        public event ReceiveParsedDataFromClientEventHandler? OnReceiveParsedData;
        /// <inheritdoc/>
        public event ClientConnectEventHandler? OnClientConnect;
        /// <inheritdoc/>
        public event ClientDisconnectEventHandler? OnClientDisconnect;

        /// <summary>
        /// 顶层通讯口
        /// </summary>
        /// <param name="physicalPort">物理口</param>
        /// <param name="getParser">获取解析器</param>
        public TopPort_M2M(IPhysicalPort_M2M physicalPort, GetParserEventHandler getParser)
        {
            PhysicalPort = physicalPort;
            PhysicalPort.OnReceiveOriginalDataFromClient += async (byte[] data, int size, Guid clientId) =>
            {
                if (_dicParsers.ContainsKey(clientId))
                {
                    if (_dicParsers.TryGetValue(clientId, out var parser))
                        await parser.ReceiveOriginalDataAsync(data, size);
                }
            };
            PhysicalPort.OnClientConnect += async clientId =>
            {
                var parser = await getParser.Invoke();
                parser.OnReceiveParsedData += async data =>
                {
                    if (OnReceiveParsedData is not null) await OnReceiveParsedData.Invoke(clientId, data);
                };
                _dicParsers.TryAdd(clientId, parser);
                if (OnClientConnect is not null) await OnClientConnect.Invoke(clientId);
            };
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            await PhysicalPort.StopAsync();
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            await PhysicalPort.StartAsync();
        }

        /// <inheritdoc/>
        public async Task SendAsync(Guid clientId, byte[] data)
        {
            await PhysicalPort.SendDataAsync(clientId, data);
            if (OnSentData is not null) await OnSentData.Invoke(data, clientId);
        }

        /// <inheritdoc/>
        public async Task SendAsync(string hostName, int port, byte[] data)
        {
            var clientId = await PhysicalPort.SendDataAsync(hostName, port, data);
            if (OnSentData is not null) await OnSentData.Invoke(data, clientId);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var task = this.CloseAsync();
            task.ConfigureAwait(false);
            task.Wait();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async Task<string?> GetClientInfos(Guid clientId)
        {
            return await PhysicalPort.GetClientInfos(clientId);
        }
    }
}
