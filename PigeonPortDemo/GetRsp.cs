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
