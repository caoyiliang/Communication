/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：BusPort.cs
********************************************************************/

using Communication.Exceptions;
using Communication.Interfaces;
using LogInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication.Bus
{
    public class BusPort : IBusPort
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<BusPort>();
        private const int BUFFER_SIZE = 8192;
        private CancellationTokenSource _closeCts;
        private TaskCompletionSource<bool> _closeTcs;
        private volatile bool _isActiveClose = true;//是否主动断开
        private SemaphoreSlim _semaphore4Write = new SemaphoreSlim(1, 1);
        private IPhysicalPort _physicalPort;
        private bool IsOpen { get => _physicalPort.IsOpen; }
        public IPhysicalPort PhysicalPort { get => _physicalPort; set { _physicalPort?.CloseAsync().Wait(); _physicalPort = value; } }

        public event ReceiveOriginalDataEventHandler OnReceiveOriginalData;

        public BusPort(IPhysicalPort physicalPort)
        {
            this._physicalPort = physicalPort ?? throw new NullReferenceException("physicalPort is null");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ConnectFailedException"></exception>
        /// <returns></returns>
        public async Task OpenAsync()
        {
            if (!_isActiveClose) return;
            _isActiveClose = false;
            try
            {
                await OpenAsync_(true);
            }
            catch (ConnectFailedException)
            {
                _isActiveClose = true;
                throw;
            }
            _ = Task.Run(async () =>
              {
                  while (!_isActiveClose)
                  {
                      await OpenAsync_();
                      await ReadBusAsync();
                  }
              });
        }

        public async Task CloseAsync()
        {
            if (_isActiveClose) return;
            _isActiveClose = true;
            _closeCts.Cancel();
            if (IsOpen)
                await _physicalPort.CloseAsync();
            if (await Task.WhenAny(_closeTcs.Task, Task.Delay(2000)) != _closeTcs.Task)
            {
                throw new TimeoutException("Waited too long to Close. timeout = 2000");
            }
        }

        public async Task SendAsync(byte[] data, int timeInterval = 0)
        {
            if (!IsOpen)
            {
                throw new NotConnectedException("Bus is not connected!");
            }
            try
            {
                try
                {
                    await _semaphore4Write.WaitAsync();
                    await _physicalPort.SendDataAsync(data, _closeCts.Token);
                    if (timeInterval > 0)
                        await Task.Delay(timeInterval);
                }
                finally
                {
                    _semaphore4Write.Release();
                }
            }
            catch (Exception e)
            {
                throw new SendException("send failed", e);
            }
        }

        private async Task ReadBusAsync()
        {
            try
            {
                while (!_closeCts.IsCancellationRequested)
                {
                    var result = await _physicalPort.ReadDataAsync(BUFFER_SIZE, _closeCts.Token);
                    if (result.Length <= 0) break;
                    try
                    {
                        await this.OnReceiveOriginalData?.Invoke(result.Data, result.Length);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Handle original data error");
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _closeCts.Cancel();
                _closeTcs.TrySetResult(true);
            }
        }

        private async Task OpenAsync_(bool isThrow = false)
        {
            if (IsOpen) return;
            while (!_isActiveClose)
            {
                try
                {
                    await _physicalPort.OpenAsync();
                    _closeCts = new CancellationTokenSource();
                    _closeTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    return;
                }
                catch (Exception ex)
                {
                    if (isThrow) throw new ConnectFailedException("bus connect failed!", ex);
                    await Task.Delay(50);
                }
            }
        }

        public void Dispose()
        {
            var task = this.CloseAsync();
            task.ConfigureAwait(false);
            task.Wait();
        }
    }
}
