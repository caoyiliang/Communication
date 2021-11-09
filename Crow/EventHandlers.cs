/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：EventHandlers.cs
********************************************************************/

namespace Crow
{
    public delegate Task SentDataEventHandler<T>(T data);

    public delegate Task ReceivedDataEventHandler<T>(T data);
}
