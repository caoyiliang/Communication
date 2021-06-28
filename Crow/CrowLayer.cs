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
 * * 说明：CrowLayer.cs
 * *
********************************************************************/

using Crow.Exceptions;
using Crow.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Crow
{
    public class CrowLayer<TReq, TRsp> : ICrowLayer<TReq, TRsp>, IDisposable
    {
        private ITilesLayer<TReq, TRsp> _port;
        private int _defaultTimeout;
        private int _timeDelayAfterSending;
        private TaskCompletionSource<bool> _startStop;
        private TaskCompletionSource<bool> _completeStop;
        private TaskCompletionSource<TRsp> _rsp;
        private ConcurrentQueue<ReqInfo> _queue;
        private volatile bool _isActive = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="defaultTimeout">默认的超时时间</param>
        /// <param name="timeDelayAfterSending">当调用SendAsync时，防止数据黏在一起，要设置一个发送时间间隔</param>
        public CrowLayer(ITilesLayer<TReq, TRsp> port, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _port = port;
            _port.OnReceiveData += async data => _rsp?.TrySetResult(data);
            _defaultTimeout = defaultTimeout;
            _timeDelayAfterSending = timeDelayAfterSending;

        }

        public event SentDataEventHandler<TReq> OnSentData;
        public event ReceivedDataEventHandler<TRsp> OnReceivedData;

        public void Dispose()
        {
            var task = this.StopAsync();
            task.ConfigureAwait(false);
            task.Wait();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        public async Task<TRsp> RequestAsync(TReq req, int timeout = -1)
        {
            return await RequestAsync(req, true, timeout);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="timeout">超时时间，当==-1时，使用构造器传入的defaultTimeout</param>
        /// <exception cref="CrowStopWorkingException">乌鸦停止工作异常</exception>
        /// <exception cref="CrowBusyException">乌鸦正忙异常</exception>
        /// <exception cref="TilesSendException">瓦片发送异常</exception>
        /// <exception cref="TimeoutException">超时异常</exception>
        /// <returns></returns>
        public async Task SendAsync(TReq req, int timeout = -1)
        {
            await RequestAsync(req, false, timeout);
        }

        public async Task StartAsync()
        {
            if (_isActive) return;

            _isActive = true;
            _queue = new ConcurrentQueue<ReqInfo>();
            _startStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (!_queue.TryDequeue(out ReqInfo data))
                    {
                        if (await Task.WhenAny(Task.Delay(100), _startStop.Task) == _startStop.Task)
                            break;
                        else
                            continue;
                    }
                    if (data.Timeout.IsCompleted)
                        continue;
                    _rsp?.TrySetCanceled();
                    _rsp = new TaskCompletionSource<TRsp>(TaskCreationOptions.RunContinuationsAsynchronously);
                    try
                    {
                        await _port.SendAsync(data.Req);
                        if (!(OnSentData is null))
                        {
                            try
                            {
                                await OnSentData(data.Req);
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        data.Rsp.TrySetException(new TilesSendException("", ex));
                        continue;
                    }
                    if (data.NeedRsp)
                    {
                        var task = await Task.WhenAny(data.Timeout, _rsp.Task, _startStop.Task);
                        if (task == _startStop.Task)
                        {
                            data.Rsp.TrySetException(new CrowStopWorkingException());
                        }
                        else if (task == _rsp.Task)
                        {
                            var rsp = await _rsp.Task;
                            data.Rsp.TrySetResult(rsp);
                            if (!(OnReceivedData is null))
                            {
                                try
                                {
                                    await OnReceivedData(rsp);
                                }
                                catch { }
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        data.Rsp.TrySetResult(default);
                        if (_startStop.Task.IsCompleted)
                            break;
                        await Task.Delay(_timeDelayAfterSending);
                    }
                }
                _completeStop.TrySetResult(true);
            });
        }

        public async Task StopAsync()
        {
            if (!_isActive) return;
            _isActive = false;
            _completeStop = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _startStop.TrySetResult(true);

            await _completeStop.Task;
        }
        private async Task<TRsp> RequestAsync(TReq req, bool needRsp, int timeout)
        {
            if (!_isActive) throw new CrowStopWorkingException();
            if (_queue.Count > 10) throw new CrowBusyException();
            var rsp = new TaskCompletionSource<TRsp>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tm = timeout == -1 ? _defaultTimeout : timeout;
            var data = new ReqInfo() { NeedRsp = needRsp, Req = req, Rsp = rsp, Timeout = Task.Delay(tm) };
            _queue.Enqueue(data);
            if (await Task.WhenAny(rsp.Task, data.Timeout) == data.Timeout)
                throw new TimeoutException($"timeout={tm}");
            return await rsp.Task;
        }
        class ReqInfo
        {
            public TReq Req { get; set; }
            public bool NeedRsp { get; set; }
            public TaskCompletionSource<TRsp> Rsp { get; set; }
            public Task Timeout { get; set; }

        }
    }
}
