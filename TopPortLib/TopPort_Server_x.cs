using Communication;
using Communication.Interfaces;
using Parser.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Channels;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    /// <summary>
    /// 顶层通讯口
    /// 融合队列，当作一个例子，可以自己去实现
    /// </summary>
    public class TopPort_Server_x : ITopPort_Server, IDisposable
    {
        private readonly ConcurrentDictionary<Guid, IParser> _dicParsers = new();
        private readonly ConcurrentDictionary<Guid, Channel<byte[]>> _dicChannels = new();
        private readonly ConcurrentDictionary<Guid, Task> _dicChannelTasks = new();

        /// <inheritdoc/>
        public IPhysicalPort_Server PhysicalPort { get; }

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
        public TopPort_Server_x(IPhysicalPort_Server physicalPort, GetParserEventHandler getParser)
        {
            PhysicalPort = physicalPort;
            PhysicalPort.OnReceiveOriginalDataFromClient += async (byte[] data, int size, Guid clientId) =>
            {
                if (_dicParsers.TryGetValue(clientId, out var parser))
                {
                    await parser.ReceiveOriginalDataAsync(data, size);
                }
            };
            PhysicalPort.OnClientConnect += async clientId =>
            {
                var parser = await getParser.Invoke();
                var channel = Channel.CreateUnbounded<byte[]>();
                var processingTask = Task.Run(async () => await ParseAndProcessDataAsync(clientId));
                parser.OnReceiveParsedData += async data =>
                {
                    await _dicChannels[clientId].Writer.WriteAsync(data);
                };
                _dicParsers.TryAdd(clientId, parser);
                _dicChannels.TryAdd(clientId, channel);
                _dicChannelTasks.TryAdd(clientId, processingTask);
                if (OnClientConnect is not null) await OnClientConnect.Invoke(clientId);
            };
            PhysicalPort.OnClientDisconnect += async clientId =>
            {
                if (_dicParsers.TryRemove(clientId, out var parser))
                {
                    if (parser is IDisposable needDisposingParser)
                    {
                        needDisposingParser.Dispose();
                    }
                }
                if (_dicChannels.TryRemove(clientId, out var channel))
                {
                    channel.Writer.Complete();
                }
                _dicChannelTasks.TryRemove(clientId, out var task);
                if (OnClientDisconnect is not null) await OnClientDisconnect.Invoke(clientId);
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

        private async Task ParseAndProcessDataAsync(Guid clientId)
        {
            await foreach (var data in _dicChannels[clientId].Reader.ReadAllAsync())
            {
                if (OnReceiveParsedData is not null)
                {
                    await OnReceiveParsedData.Invoke(clientId, data);
                }
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync(Guid clientId, byte[] data)
        {
            await PhysicalPort.SendDataAsync(clientId, data);
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
