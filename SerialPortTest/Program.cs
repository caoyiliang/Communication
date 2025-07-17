namespace SerialPortTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            PortTest test = new PortTest();
            await test.Open();

            while (string.IsNullOrWhiteSpace(Console.ReadLine()))
            {
                await test.SendAsync();
            }

            Console.ReadLine();
        }
    }
}
