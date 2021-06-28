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
 * * 说明：HeadLengthParser.cs
 * *
********************************************************************/

using LogInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public class HeadLengthParser : BaseParser
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<HeadLengthParser>();
        private int _startIndex = -1;
        private int _length = -1;
        /// <summary>
        /// 帧头
        /// </summary>
        private byte[] _head;
        private GetDataLengthEventHandler OnGetDataLength;
        /// <summary>
        /// 长度为除了帧头之外的所有数据的长度
        /// </summary>
        /// <param name="head"></param>
        /// <param name="getDataLength"></param>
        public HeadLengthParser(byte[] head, GetDataLengthEventHandler getDataLength)
        {
            if (head == null || head.Length == 0) throw new Exception("必须传入帧头");
            this._head = head;
            this.OnGetDataLength = getDataLength ?? throw new Exception("必须要getDataLength");
        }

        /// <summary>
        ///  长度为包括表示长度的字节在内的所有数据的长度
        /// </summary>
        /// <param name="getDataLength"></param>
        public HeadLengthParser(GetDataLengthEventHandler getDataLength)
        {
            this._head = new byte[0];
            this.OnGetDataLength = getDataLength ?? throw new Exception("必须要getDataLength");
        }

        protected override async Task<bool> CanFindEndIndexAsync()
        {
            if (_bytes.Count - (_startIndex - _bytes.StartIndex) < _bytes.GetCurrentMessageLength()) return false;
            if (_length == -1)
            {
                byte[] temp = new byte[_bytes.Count - (_startIndex - _bytes.StartIndex)];
                Array.Copy(_bytes.Bytes, _startIndex, temp, 0, temp.Length);
                try
                {
                    var rsp = await OnGetDataLength(temp);
                    if (rsp.ErrorCode != Parser.ErrorCode.Success)
                    {
                        return false;
                    }
                    _length = rsp.Length;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Get data length error");
                    return false;
                }
                if (_length <= 0)
                    throw new Exception("数据长度必须大于0");
            }
            if (_head.Length + _length > _bytes.Count - (_startIndex - _bytes.StartIndex))
            {
                _bytes.SetCurrentMessageLength(_head.Length + _length);
                return false;
            }
            return true;
        }

        protected override int FindStartIndex()
        {
            if (_startIndex == -1)
            {
                var rsp = FindIndex(_bytes.StartIndex, _head);
                //为了避免存储太多的垃圾数据，要清除垃圾
                if (rsp.Code == ErrorCode.NotFound)
                {
                    _bytes.RemoveHeader(_bytes.Count - _head.Length);
                }
                else if (rsp.Code == ErrorCode.Success)
                {
                    _startIndex = rsp.Index;
                }
            }
            return _startIndex;
        }

        protected override int FindEndIndex()
        {
            return _startIndex + _head.Length + _length;
        }

        protected override void ResetSatrtIndex(int bytesOldIndex)
        {
            if (_startIndex != -1)
                _startIndex -= bytesOldIndex;
        }

        protected override async Task<bool> ReceiveOneFrameAsync()
        {
            if (await base.ReceiveOneFrameAsync())
            {
                _startIndex = -1;
                _length = -1;
                _bytes.SetCurrentMessageLength(-1);
                return true;
            }
            return false;
        }
    }
}
