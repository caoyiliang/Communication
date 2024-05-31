﻿namespace TopPortLib.Interfaces;

/// <summary>
/// 返回约束
/// </summary>
/// <typeparam name="T">返回类型</typeparam>
public interface IAsyncResponse_Server<T>
{
    /// <summary>
    /// 检测命令是否是该返回
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="bytes">收到的命令</param>
    /// <returns>是否是该返回</returns>
    (bool Type, byte[]? CheckBytes) Check(int clientId, byte[] bytes);

    /// <summary>
    /// 从命令中解析有用数据
    /// </summary>
    /// <param name="clientId">客户端ID</param>
    /// <param name="bytes">收到的命令</param>
    Task AnalyticalData(int clientId, byte[] bytes);

    /// <summary>
    /// 获取返回数据
    /// </summary>
    /// <returns>返回数据</returns>
    T GetResult();
}