// See https://aka.ms/new-console-template for more information
using Communication.Bus;

Console.WriteLine("Hello, World!");

var tcpServer = new TcpServer("127.0.0.1", 2756);
tcpServer.OnClientConnect += TcpServer_ClientConnect;
tcpServer.OnClientDisconnect += TcpServer_ClientDisconnect;
tcpServer.OnReceiveOriginalDataFromClient += TcpServer_ReceiveOriginalDataFromTcpClient;
await tcpServer.StartAsync();

async Task TcpServer_ReceiveOriginalDataFromTcpClient(byte[] data, int size, int clientId)
{
    var tmp = new byte[size];
    Array.Copy(data, 0, tmp, 0, size);
    await tcpServer!.SendDataAsync(clientId, tmp);
    if (data[0] == 0x89)
    {
        await tcpServer.DisconnectClientAsync(clientId);
    }
}

async Task TcpServer_ClientConnect(int clientId)
{
    await tcpServer!.SendDataAsync(clientId, [0x01, 0x0d]);
    var info = await ((TcpServer)tcpServer!).GetClientInfo(clientId);

    if (info.HasValue)
    {

        var clients = await ((TcpServer)tcpServer!).GetClientsByIp(info.Value.IPAddress);

        await ((TcpServer)tcpServer).GetClientId(info.Value.IPAddress, info.Value.Port);
    }
}

async Task TcpServer_ClientDisconnect(int clientId)
{
    var info = await ((TcpServer)tcpServer!).GetClientInfo(clientId);
}

Console.ReadKey();