using System.Reflection;
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
using static PigeonPortProtocolDemo.PigeonPortProtocolDemo;

namespace PigeonPortProtocolDemo;

public class PigeonPortProtocolDemo : IPigeonPortProtocolDemo
{
    private static readonly ILogger _logger = Logs.LogFactory.GetLogger<PigeonPortProtocolDemo>();
    private readonly IPigeonPort _crowPort;
    internal static readonly byte[] Foot = new byte[] { 0x0d };

    private bool _isConnect = false;
    public bool IsConnect => _isConnect;

    public event ActivelyPushDataEventHandler<(List<decimal> recData, int result)>? OnReadValue;

    /// <inheritdoc/>
    public event DisconnectEventHandler? OnDisconnect { add => _crowPort.OnDisconnect += value; remove => _crowPort.OnDisconnect -= value; }
    /// <inheritdoc/>
    public event ConnectEventHandler? OnConnect { add => _crowPort.OnConnect += value; remove => _crowPort.OnConnect -= value; }

    public PigeonPortProtocolDemo(TcpClient serialPort, int defaultTimeout = 5000)
    {
        _crowPort = new PigeonPort(this, new TopPort(serialPort, new FootParser(Foot)), defaultTimeout);
        _crowPort.OnReceiveActivelyPushData += _crowPort_OnReceiveActivelyPushData;
        _crowPort.OnSentData += CrowPort_OnSentData;
        _crowPort.OnReceivedData += CrowPort_OnReceivedData;
        _crowPort.OnConnect += _crowPort_OnConnect;
        _crowPort.OnDisconnect += _crowPort_OnDisconnect;
    }

    private async Task _crowPort_OnReceiveActivelyPushData(Type type, object data)
    {
        //可不在此处处理
        await Task.CompletedTask;
    }

    private async Task _crowPort_OnDisconnect()
    {
        _isConnect = false;
        await Task.CompletedTask;
    }

    private async Task _crowPort_OnConnect()
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
    public Task OpenAsync() => _crowPort.StartAsync();

    /// <inheritdoc/>
    public Task CloseAsync() => _crowPort.StopAsync();

    /// <inheritdoc/>
    public async Task<List<decimal>?> ReadSignalValueAsync(string address = "01", int tryCount = 0, int timeOut = -1, CancellationTokenSource? cancelToken = null)
    {
        if (!_isConnect) throw new NotConnectedException();
        return await ProcessUtils.ReTry(async () => (await _crowPort.RequestAsync<ReadValueReq, ReadValueRsp>(new ReadValueReq(address), timeOut)).RecData, tryCount, cancelToken);
    }


    private async Task ReadValueRspEvent((List<decimal> recData, int result) rs)
    {
        if (OnReadValue is not null)
            await OnReadValue(rs);
    }
}