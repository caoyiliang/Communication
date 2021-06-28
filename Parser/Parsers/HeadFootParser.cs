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
 * * 说明：HeadFootParser.cs
 * *
********************************************************************/

using Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public class HeadFootParser : BaseParser, IParser
    {
        private int _startIndex = -1;
        /// <summary>
        /// 帧头
        /// </summary>
        private byte[] _head;
        /// <summary>
        /// 帧尾
        /// </summary>
        private byte[] _foot;

        public HeadFootParser(byte[] head, byte[] foot)
        {
            if (head == null || head.Length == 0) throw new Exception("必须传入帧头");
            if (foot == null || foot.Length == 0) throw new Exception("必须传入帧尾");
            this._head = head;
            this._foot = foot;
        }

        protected override async Task<bool> ReceiveOneFrameAsync()
        {
            if (await base.ReceiveOneFrameAsync())
            {
                _startIndex = -1;
                return true;
            }
            return false;
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
            var rsp = FindIndex(_startIndex + _head.Length, _foot);
            if (rsp.Code != ErrorCode.Success) return -1;
            return rsp.Index + _foot.Length;
        }

        protected override void ResetSatrtIndex(int bytesOldIndex)
        {
            if (_startIndex != -1)
                _startIndex -= bytesOldIndex;
        }
    }
}
