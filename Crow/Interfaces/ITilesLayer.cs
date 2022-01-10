namespace Crow.Interfaces
{
    /// <summary>
    /// 瓦片层
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <typeparam name="TRsp"></typeparam>
    public interface ITilesLayer<TReq, TRsp>
    {
        event Action<TRsp> OnReceiveData;
        Task SendAsync(TReq data);
    }
}
