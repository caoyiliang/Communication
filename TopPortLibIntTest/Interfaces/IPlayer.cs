using TopPortLibIntTest.Request;
using TopPortLibIntTest.Response;

namespace TopPortLibIntTest.Interfaces
{
    interface IPlayer
    {
        Task<PlayerRsp> Play(PlayerReq req);
    }
}
