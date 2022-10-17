using Communication.Bus.PhysicalPort;
using Crow.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.Parsers;
using TopPortLib;
using TopPortLib.Interfaces;
using TopPortLibIntTest.Interfaces;
using TopPortLibIntTest.Request;

namespace TopPortLibIntTest
{
    [TestClass]
    public class CrowPortTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            ICrowPort crowPort = new CrowPort(new SerialPort(), new TimeParser());
            await crowPort.OpenAsync();
            IPlayer player = new Player(crowPort);
            try
            {
                var rsp = await player.Play(new PlayerReq(9527, "1234"));
            }
            catch (TimeoutException)
            {

            }
            catch (CrowBusyException)
            {

            }
        }
    }
}
