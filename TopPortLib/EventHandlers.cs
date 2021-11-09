/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：EventHandlers.cs
********************************************************************/

namespace TopPortLib
{
    public delegate Task RequestedDataEventHandler(byte[] data);

    public delegate Task RespondedDataEventHandler(byte[] data);

    public delegate Task ReceiveResponseDataEventHandler(Type type, object data);
}
