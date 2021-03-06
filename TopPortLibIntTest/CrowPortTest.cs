﻿/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：CrowPortTest.cs
********************************************************************/

using Communication.Bus.PhysicalPort;
using Crow.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.Parsers;
using System;
using System.Threading.Tasks;
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
                var rsp = await player.Play(new PlayerReq() { ID = 9527, Name = "����֮��" });
            }
            catch (TimeoutException ex)
            {

            }
            catch (CrowBusyException ex)
            {

            }
        }
    }
}
