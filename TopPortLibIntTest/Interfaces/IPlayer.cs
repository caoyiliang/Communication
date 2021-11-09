/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：IPlayer.cs
********************************************************************/

using TopPortLibIntTest.Request;
using TopPortLibIntTest.Response;

namespace TopPortLibIntTest.Interfaces
{
    interface IPlayer
    {
        Task<PlayerRsp> Play(PlayerReq req);
    }
}
