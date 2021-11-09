/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：ITilesLayer.cs
********************************************************************/

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
