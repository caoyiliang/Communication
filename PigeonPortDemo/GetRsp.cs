using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopPortLib;

namespace PigeonPortDemo
{
    class GetRsp
    {
        public bool Success { get; set; }
        public GetRsp(byte[] rspBytes)
        {
            if (rspBytes[0] == 0)
                Success = true;
            else
                Success = false;
        }
    }
}
