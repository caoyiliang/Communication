namespace Parser.Interfaces
{
    public interface IParser
    {
        event ReceiveParsedDataEventHandler OnReceiveParsedData;

        Task ReceiveOriginalDataAsync(byte[] data, int size);
    }
}
