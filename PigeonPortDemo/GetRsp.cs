/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：GetRsp.cs
********************************************************************/

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
