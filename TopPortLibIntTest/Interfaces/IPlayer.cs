using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TopPortLibIntTest.Request;
using TopPortLibIntTest.Response;

namespace TopPortLibIntTest.Interfaces
{
    interface IPlayer
    {
        Task<PlayerRsp> Play(PlayerReq req);
    }
}
