using TopPortLib.Interfaces;
using TopPortLibIntTest.Interfaces;
using TopPortLibIntTest.Request;
using TopPortLibIntTest.Response;

namespace TopPortLibIntTest
{
    class Player : IPlayer
    {
        private readonly ICrowPort _crowPort;
        public Player(ICrowPort crowPort)
        {
            _crowPort = crowPort;
        }
        public async Task<PlayerRsp> Play(PlayerReq req)
        {
            return await _crowPort.RequestAsync<PlayerReq, PlayerRsp>(req);
        }
    }
}
