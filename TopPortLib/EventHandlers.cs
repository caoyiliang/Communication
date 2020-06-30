using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TopPortLib
{
    public delegate Task RequestedDataEventHandler(byte[] data);

    public delegate Task RespondedDataEventHandler(byte[] data);

    public delegate Task ReceiveResponseDataEventHandler(Type type, object data);
}
