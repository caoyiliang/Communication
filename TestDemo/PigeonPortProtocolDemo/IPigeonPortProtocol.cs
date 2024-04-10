using Communication;

namespace PigeonPortProtocolDemo;

public interface IPigeonPortProtocol
{
    /// <summary>
    /// 设备是否连接
    /// </summary>
    public bool IsConnect { get; }

    /// <summary>
    /// 打开串口
    /// </summary>
    Task OpenAsync();

    /// <summary>
    /// 关闭串口
    /// </summary>
    Task CloseAsync();

    /// <summary>
    /// 主动上传的数据比如读浓度
    /// </summary>
    event ActivelyPushDataEventHandler<(List<decimal> recData, int result)>? OnReadValue;

    /// <summary>
    /// 对端掉线
    /// </summary>
    event DisconnectEventHandler? OnDisconnect;

    /// <summary>
    /// 对端连接成功
    /// </summary>
    event ConnectEventHandler? OnConnect;

    /// <summary>
    /// 读信号量
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="tryCount">重试次数</param>
    /// <param name="timeOut">超时时间(-1则使用构造传入超时)</param>
    /// <param name="cancelToken">取消</param>
    /// <returns>信号量值</returns>
    Task<List<decimal>?> ReadSignalValueAsync(string address = "01", int tryCount = 0, int timeOut = -1, CancellationToken cancelToken = default);
}
