/********************************************************************
 * *
 * * 使本项目源码或本项目生成的DLL前请仔细阅读以下协议内容，如果你同意以下协议才能使用本项目所有的功能，
 * * 否则如果你违反了以下协议，有可能陷入法律纠纷和赔偿，作者保留追究法律责任的权利。
 * *
 * * 1、你可以在开发的软件产品中使用和修改本项目的源码和DLL，但是请保留所有相关的版权信息。
 * * 2、不能将本项目源码与作者的其他项目整合作为一个单独的软件售卖给他人使用。
 * * 3、不能传播本项目的源码和DLL，包括上传到网上、拷贝给他人等方式。
 * * 4、以上协议暂时定制，由于还不完善，作者保留以后修改协议的权利。
 * *
 * * Copyright ©2013-? yzlm Corporation All rights reserved.
 * * 作者： 曹一梁 QQ：347739303
 * * 请保留以上版权信息，否则作者将保留追究法律责任。
 * *
 * * 创建时间：2021-06-28
 * * 说明：TcpServer.cs
 * *
********************************************************************/

using Communication.Interfaces;
using LogInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Communication.Bus
{
    public class TcpServer : ITcpServer
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<TcpServer>();
        private string _hostName;
        private int _port;
        private int _bufferSize;
        private TcpListener _listener;
        private CancellationTokenSource _stopCts;
        private TaskCompletionSource<bool> _stopTcs;
        private ConcurrentDictionary<int, TcpClient> _dicClients = new ConcurrentDictionary<int, TcpClient>();
        public bool IsActive { get; private set; }

        public event ReceiveOriginalDataFromTcpClientEventHandler OnReceiveOriginalDataFromTcpClient;
        public event ClientConnectEventHandler OnClientConnect;
        public event ClientDisconnectEventHandler OnClientDisconnect;

        public TcpServer(string hostName, int port, int bufferSize = 8192)
        {
            this._hostName = hostName;
            this._port = port;
            this._bufferSize = bufferSize;
        }

        public async Task StartAsync()
        {
            if (this.IsActive) return;

            _listener = new TcpListener(IPAddress.Parse(_hostName), _port);
            _listener.Start();
            _stopCts = new CancellationTokenSource();
            _stopTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IsActive = true;
            await AcceptClientAsync();
        }

        public async Task StopAsync()
        {
            if (!this.IsActive) return;
            this._stopCts.Cancel();
            this._listener.Stop();
            if (await Task.WhenAny(_stopTcs.Task, Task.Delay(2000)) != _stopTcs.Task)
            {
                throw new TimeoutException("Waited too long to Close. timeout = 2000");
            }
        }

        public async Task SendDataAsync(int clientId, byte[] data)
        {
            try
            {
                if (!_dicClients.TryGetValue(clientId, out TcpClient client)) return;
                var stream = client.GetStream();
                lock (stream)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Send data");
            }
        }

        public async Task DisconnectClientAsync(int clientId)
        {
            if (!_dicClients.TryGetValue(clientId, out TcpClient client)) return;
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Disconnect Client Async error{clientId}");
            }
            await Task.CompletedTask;
        }

        private async Task AcceptClientAsync()
        {
            _ = Task.Run(async () =>
            {
                var clientCounter = 0;
                try
                {
                    while (!_stopCts.IsCancellationRequested)
                    {
                        TcpClient client;
                        try
                        {
                            client = await _listener.AcceptTcpClientAsync();
                        }
                        catch (SocketException)
                        {
                            continue;
                        }
                        int clientId = clientCounter;
                        clientCounter++;
                        _dicClients.TryAdd(clientId, client);
                        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        try
                        {
                            await this.OnClientConnect?.Invoke(remoteEndPoint.Address.ToString(), remoteEndPoint.Port, clientId);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Handle client connect error");
                        }
                        _ = Task.Run(async () => await HandleClientAsync(client, clientId));
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Handle client connect error");
                }
                finally
                {
                    _stopCts.Cancel();
                    try
                    {
                        _listener.Stop();
                        _listener.Server.Close();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex);
                    }
                    foreach (var c in _dicClients.Values)
                    {
                        c.Close();
                    }
                    _dicClients.Clear();
                    _stopTcs.TrySetResult(true);
                    this.IsActive = false;
                }
            });
            await Task.CompletedTask;
        }

        private async Task HandleClientAsync(TcpClient client, int clientId)
        {
            try
            {
                using (var clientStream = client.GetStream())
                using (client)
                {
                    var amountRead = 0;
                    var buf = new byte[this._bufferSize];
                    while (!this._stopCts.IsCancellationRequested && client.Connected)
                    {
                        amountRead = await clientStream.ReadAsync(buf, 0, buf.Length, _stopCts.Token);
                        if (amountRead <= 0)
                            break;
                        try
                        {
                            await OnReceiveOriginalDataFromTcpClient?.Invoke(buf, amountRead, clientId);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Handle original data error");
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _dicClients.TryRemove(clientId, out TcpClient value);
                try
                {
                    await this.OnClientDisconnect?.Invoke(clientId);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Handle client disconnect error");
                }
            }
        }
    }
}
