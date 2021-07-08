/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：EventHandlers.cs
********************************************************************/

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
