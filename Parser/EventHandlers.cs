/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：EventHandlers.cs
********************************************************************/

namespace Parser
{
    public delegate Task ReceiveParsedDataEventHandler(byte[] data);

    public delegate Task<GetDataLengthRsp> GetDataLengthEventHandler(byte[] data);

    public class GetDataLengthRsp
    {
        public int Length { get; set; }
        public ErrorCode ErrorCode { get; set; }
    }

    public enum ErrorCode
    {
        Success,
        LengthNotEnough
    }
}
