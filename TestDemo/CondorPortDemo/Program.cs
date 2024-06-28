// See https://aka.ms/new-console-template for more information

using Communication.Bus;
using CondorPortProtocolDemo;

Console.WriteLine("Hello, World!");
ICondorPortProtocol condorPortProtocolDemox = new CondorPortProtocol(new TcpServer("0.0.0.0", 2756));
condorPortProtocolDemox.OnReadValue += CondorPortProtocolDemox_OnReadValue;
condorPortProtocolDemox.OnClientConnect += CondorPortProtocolDemox_OnClientConnect;
async Task CondorPortProtocolDemox_OnClientConnect(Guid clientId)
{
    try
    {
        await condorPortProtocolDemox.ReadSignalValueAsync(clientId);
    }
    catch (Exception)
    {

    }

    await Task.CompletedTask;
}

static async Task CondorPortProtocolDemox_OnReadValue(Guid clientId, (List<decimal> recData, int result) objects) => await Task.CompletedTask;

await condorPortProtocolDemox.StartAsync();



Console.ReadKey();
