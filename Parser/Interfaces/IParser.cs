using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Interfaces
{
    public interface IParser
    {
        event ReceiveParsedDataEventHandler OnReceiveParsedData;

        Task ReceiveOriginalDataAsync(byte[] data, int size);
    }
}
