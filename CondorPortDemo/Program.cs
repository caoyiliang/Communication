// See https://aka.ms/new-console-template for more information

using Communication.Bus;
using CondorPortProtocolDemo;

Console.WriteLine("Hello, World!");
ICondorPortProtocol condorPortProtocolDemox = new CondorPortProtocol(new TcpServer("0.0.0.0", 2756));
condorPortProtocolDemox.OnReadValue += CondorPortProtocolDemox_OnReadValue;

async Task CondorPortProtocolDemox_OnReadValue(int clientId, (List<decimal> recData, int result) objects)
{
    await Task.CompletedTask;
}

await condorPortProtocolDemox.StartAsync();

//await condorPortProtocolDemox.ReadSignalValueAsync(0);

Console.ReadKey();
