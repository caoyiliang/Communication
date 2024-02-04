using Communication;
using Communication.Bus;
using CondorPortProtocolDemo.Request;
using CondorPortProtocolDemo.Response;
using LogInterface;
using Parser.Parsers;
using System.Xml.Linq;
using TopPortLib;
using TopPortLib.Interfaces;
using Utils;

namespace CondorPortProtocolDemo
{
    public class CondorPortProtocol : ICondorPortProtocol
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<CondorPortProtocol>();
        private readonly ICondorPort _condorPort;
        internal static readonly byte[] Foot = new byte[] { 0x0d };
        private bool _isListened = false;
        public bool IsListened => _isListened;

        public event ActivelyPushDataServerEventHandler<(List<decimal> recData, int result)>? OnReadValue;
        public event ClientConnectEventHandler? OnClientConnect { add => _condorPort.OnClientConnect += value; remove => _condorPort.OnClientConnect -= value; }
        public event ClientDisconnectEventHandler? OnClientDisconnect { add => _condorPort.OnClientDisconnect += value; remove => _condorPort.OnClientDisconnect -= value; }

        public CondorPortProtocol(TcpServer serverPort, int defaultTimeout = 5000)
        {
            _condorPort = new CondorPort(this, new TopPort_Server(serverPort, async () => await Task.FromResult(new FootParser(Foot))), defaultTimeout);
            _condorPort.OnReceiveActivelyPushData += CondorPort_OnReceiveActivelyPushData;
            _condorPort.OnSentData += CondorPort_OnSentData;
            _condorPort.OnReceivedData += CondorPort_OnReceivedData;
        }

        private async Task CondorPort_OnReceivedData(int clientId, byte[] data)
        {
            var info = await ((TcpServer)_condorPort.PhysicalPort).GetClientInfo(clientId);
            if (!info.HasValue) return;
            _logger.Trace($"CondorPortProtocolDemo {info.Value.IPAddress}:{info.Value.Port} Rec:<-- {StringByteUtils.BytesToString(data)}");
            await Task.CompletedTask;
        }

        private async Task CondorPort_OnSentData(int clientId, byte[] data)
        {
            var info = await ((TcpServer)_condorPort.PhysicalPort).GetClientInfo(clientId);
            if (!info.HasValue) return;
            _logger.Trace($"CondorPortProtocolDemo {info.Value.IPAddress}:{info.Value.Port} Send:--> {StringByteUtils.BytesToString(data)}");
            await Task.CompletedTask;
        }

        private async Task CondorPort_OnReceiveActivelyPushData(int clientId, Type type, object data)
        {
            //可不在此处处理
            await Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            await _condorPort.StopAsync();
            _isListened = false;
        }

        public async Task StartAsync()
        {
            await _condorPort.StartAsync();
            _isListened = false;
        }

        public async Task<List<decimal>?> ReadSignalValueAsync(int clientId, int tryCount = 0, int timeOut = -1, CancellationTokenSource? cancelToken = null)
        {
            return (await _condorPort.RequestAsync<ReadValueReq, ReadValueRsp>(clientId, new ReadValueReq(), timeOut).ReTry(tryCount, cancelToken))?.RecData;
        }

        private async Task ReadValueRspEvent(int clientId, (List<decimal> recData, int result) rs)
        {
            if (OnReadValue is not null)
                await OnReadValue(clientId, rs);
        }
    }
}