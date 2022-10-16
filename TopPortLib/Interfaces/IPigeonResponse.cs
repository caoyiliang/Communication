namespace TopPortLib.Interfaces;

/// <summary>
/// 返回约束
/// </summary>
/// <typeparam name="T">返回类型</typeparam>
public interface IPigeonResponse<T>
{
    /// <summary>
    /// 检测命令是否是该返回
    /// </summary>
    /// <param name="bytes">收到的命令</param>
    /// <returns>是否是该返回</returns>
    bool Check(byte[] bytes);

    /// <summary>
    /// 从命令中解析有用数据
    /// </summary>
    /// <param name="bytes">收到的命令</param>
    Task AnalyticalData(byte[] bytes);

    /// <summary>
    /// 获取返回数据
    /// </summary>
    /// <returns>返回数据</returns>
    T GetResult();
}
