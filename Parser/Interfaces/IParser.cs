/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：IParser.cs
********************************************************************/

namespace Parser.Interfaces
{
    public interface IParser
    {
        event ReceiveParsedDataEventHandler OnReceiveParsedData;

        Task ReceiveOriginalDataAsync(byte[] data, int size);
    }
}
