using Communication;
using Communication.Bus.PhysicalPort;
using Communication.Exceptions;
using LogInterface;
using Parser.Parsers;
using PigeonPortProtocolDemo.Request;
using PigeonPortProtocolDemo.Response;
using TopPortLib;
using TopPortLib.Interfaces;
using Utils;

namespace PigeonPortProtocolDemo;

public class PigeonPortProtocol : IPigeonPortProtocol
{
    private static readonly ILogger _logger = Logs.LogFactory.GetLogger<PigeonPortProtocol>();
    private readonly IPigeonPort _pigeonPort;
    internal static readonly byte[] Foot = new byte[] { 0x0d };

    private bool _isConnect = false;
    public bool IsConnect => _isConnect;

    public event ActivelyPushDataEventHandler<(List<decimal> recData, int result)>? OnReadValue;

    /// <inheritdoc/>
    public event DisconnectEventHandler? OnDisconnect { add => _pigeonPort.OnDisconnect += value; remove => _pigeonPort.OnDisconnect -= value; }
    /// <inheritdoc/>
    public event ConnectEventHandler? OnConnect { add => _pigeonPort.OnConnect += value; remove => _pigeonPort.OnConnect -= value; }

    public PigeonPortProtocol(TcpClient serialPort, int defaultTimeout = 5000)
    {
        _pigeonPort = new PigeonPort(this, new TopPort(serialPort, new FootParser(Foot)), defaultTimeout);
        _pigeonPort.OnReceiveActivelyPushData += CrowPort_OnReceiveActivelyPushData;
        _pigeonPort.OnSentData += CrowPort_OnSentData;
        _pigeonPort.OnReceivedData += CrowPort_OnReceivedData;
        _pigeonPort.OnConnect += CrowPort_OnConnect;
        _pigeonPort.OnDisconnect += CrowPort_OnDisconnect;
    }

    private async Task CrowPort_OnReceiveActivelyPushData(Type type, object data)
    {
        //可不在此处处理
        await Task.CompletedTask;
    }

    private async Task CrowPort_OnDisconnect()
    {
        _isConnect = false;
        await Task.CompletedTask;
    }

    private async Task CrowPort_OnConnect()
    {
        _isConnect = true;
        await Task.CompletedTask;
    }

    private async Task CrowPort_OnReceivedData(byte[] data)
    {
        _logger.Trace($"PigeonPortProtocolDemo Rec:<-- {data}");
        await Task.CompletedTask;
    }

    private async Task CrowPort_OnSentData(byte[] data)
    {
        _logger.Trace($"PigeonPortProtocolDemo Send:--> {data}");
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OpenAsync() => _pigeonPort.StartAsync();

    /// <inheritdoc/>
    public Task CloseAsync() => _pigeonPort.StopAsync();

    /// <inheritdoc/>
    public async Task<List<decimal>?> ReadSignalValueAsync(string address = "01", int tryCount = 0, int timeOut = -1, CancellationTokenSource? cancelToken = null)
    {
        if (!_isConnect) throw new NotConnectedException();
        return await ProcessUtils.ReTry(async () => (await _pigeonPort.RequestAsync<ReadValueReq, ReadValueRsp>(new ReadValueReq(address), timeOut)).RecData, tryCount, cancelToken);
    }


    private async Task ReadValueRspEvent((List<decimal> recData, int result) rs)
    {
        if (OnReadValue is not null)
            await OnReadValue(rs);
    }
}