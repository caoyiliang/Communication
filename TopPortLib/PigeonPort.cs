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
 * * 说明：PigeonPort.cs
 * *
********************************************************************/

using Communication.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TopPortLib.Exceptions;
using TopPortLib.Interfaces;

namespace TopPortLib
{
    public class PigeonPort : IPigeonPort
    {
        private ITopPort _topPort;
        private int _defaultTimeout;
        private int _timeDelayAfterSending;
        private Func<byte[], Type> _getRspTypeByRspBytes;
        private List<ReqInfo> _reqInfos = new List<ReqInfo>();
        public event RequestedDataEventHandler OnRequestedData;
        public event RespondedDataEventHandler OnRespondedData;
        public event ReceiveResponseDataEventHandler OnReceiveResponseData;

        public IPhysicalPort PhysicalPort { get => _topPort.PhysicalPort; set => _topPort.PhysicalPort = value; }
        public PigeonPort(ITopPort topPort, Func<byte[], Type> getRspTypeByRspBytes, int defaultTimeout = 5000, int timeDelayAfterSending = 20)
        {
            _topPort = topPort;
            _topPort.OnReceiveParsedData += _topPort_OnReceiveParsedData;
            _getRspTypeByRspBytes = getRspTypeByRspBytes;
            _defaultTimeout = defaultTimeout;
            _timeDelayAfterSending = timeDelayAfterSending;
        }

        private async Task _topPort_OnReceiveParsedData(byte[] data)
        {
            await RespondedDataAsync(data);
            Type rspType;
            try
            {
                rspType = _getRspTypeByRspBytes(data);
            }
            catch (Exception ex)
            {
                throw new GetRspTypeByRspBytesFailedException("通过响应的字节来获取响应类型失败", ex);
            }
            object rsp = null;
            try
            {
                var constructors = rspType.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var args = constructor.GetParameters();
                    if (args.Length == 1)
                    {
                        rsp = constructor.Invoke(new object[] { data });
                    }
                }
                if (rsp == null)
                    throw new ResponseParameterCreateFailedException("缺少一个参数的构造器");
            }
            catch (Exception ex)
            {
                throw new ResponseParameterCreateFailedException("ResponseParameterCreateFailedException", ex);
            }
            ReqInfo reqInfo;
            lock (_reqInfos)
            {
                reqInfo = _reqInfos.Find(ri => ri.RspType == rspType);
            }
            if (reqInfo != null)
            {
                reqInfo.TaskCompletionSource.TrySetResult(rsp);
                return;
            }
            if (this.OnReceiveResponseData != null)
            {
                try
                {
                    await OnReceiveResponseData(rspType, rsp);
                }
                catch
                {
                }
            }
        }

        public async Task<TRsp> RequestAsync<TReq, TRsp>(TReq req, int timeout = -1) where TReq : IByteStream
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reqInfo = new ReqInfo()
            {
                RspType = typeof(TRsp),
                TaskCompletionSource = tcs,
            };
            lock (_reqInfos)
            {
                _reqInfos.Add(reqInfo);
            }
            var timeoutTask = Task.Delay(to);
            var bytes = req.ToBytes();
            try
            {
                var sendTask = _topPort.SendAsync(bytes, _timeDelayAfterSending);
                if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
                    throw new TimeoutException($"timeout={to}");
                await sendTask;
                await RequestDataAsync(bytes);
                if (timeoutTask == await Task.WhenAny(timeoutTask, tcs.Task))
                    throw new TimeoutException($"timeout={to}");
                return (TRsp)await tcs.Task;
            }
            finally
            {
                lock (_reqInfos)
                {
                    _reqInfos.Remove(reqInfo);
                }
            }
        }

        public async Task SendAsync<TReq>(TReq req, int timeout = -1) where TReq : IByteStream
        {
            var to = timeout == -1 ? _defaultTimeout : timeout;
            var timeoutTask = Task.Delay(to);
            var bytes = req.ToBytes();
            var sendTask = _topPort.SendAsync(bytes, _timeDelayAfterSending);
            if (timeoutTask == await Task.WhenAny(timeoutTask, sendTask))
                throw new TimeoutException($"timeout={to}");
            await sendTask;
            await RequestDataAsync(bytes);
        }
        private async Task RequestDataAsync(byte[] data)
        {
            if (this.OnRequestedData != null)
            {
                try
                {
                    await OnRequestedData(data);
                }
                catch
                {
                }
            }
        }
        private async Task RespondedDataAsync(byte[] data)
        {
            if (this.OnRespondedData != null)
            {
                try
                {
                    await OnRespondedData(data);
                }
                catch
                {
                }
            }
        }
        public async Task StartAsync()
        {
            await _topPort.OpenAsync();
        }

        public async Task StopAsync()
        {
            await _topPort.CloseAsync();
            lock (_reqInfos)
            {
                _reqInfos.Clear();
            }
        }
        class ReqInfo
        {
            public Type RspType { get; set; }
            public TaskCompletionSource<object> TaskCompletionSource { get; set; }
        }
    }
}
