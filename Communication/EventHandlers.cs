/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：EventHandlers.cs
********************************************************************/

namespace Communication
{
    public delegate Task ReceiveOriginalDataEventHandler(byte[] data, int size);

    public delegate Task ReceiveOriginalDataFromTcpClientEventHandler(byte[] data, int size, int clientId);

    public delegate Task ClientConnectEventHandler(string hostName, int port, int clientId);

    public delegate Task NamedPipeClientConnectEventHandler(int clientId);

    public delegate Task ClientDisconnectEventHandler(int clientId);
}
