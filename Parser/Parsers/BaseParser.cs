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
 * * 说明：BaseParser.cs
 * *
********************************************************************/

using LogInterface;
using Parser.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Parser.Parsers
{
    public abstract class BaseParser : IParser
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger("BaseParser");
        protected RemainBytes _bytes = new RemainBytes();

        public event ReceiveParsedDataEventHandler OnReceiveParsedData;

        public async Task ReceiveOriginalDataAsync(byte[] data, int size)
        {
            var bytesOldIndex = _bytes.StartIndex;
            _bytes.Append(data, 0, size);
            if (bytesOldIndex != 0 && _bytes.StartIndex == 0)
            {
                ResetSatrtIndex(bytesOldIndex);
            }
            while (await ReceiveOneFrameAsync()) ;
        }
        protected virtual void ResetSatrtIndex(int bytesOldIndex) { }

        protected virtual async Task<bool> CanFindEndIndexAsync() => await Task.FromResult(true);

        protected async virtual Task<bool> ReceiveOneFrameAsync()
        {
            int startIndex = FindStartIndex();
            if (startIndex < 0)
            {
                return false;
            }
            if (!await CanFindEndIndexAsync()) return false;
            int endIndex = FindEndIndex();
            if (endIndex < 0)
            {
                return false;
            }
            byte[] data = new byte[endIndex - startIndex];
            Array.Copy(_bytes.Bytes, startIndex, data, 0, data.Length);
            _bytes.RemoveHeader(endIndex - _bytes.StartIndex);
            try
            {
                await this.OnReceiveParsedData?.Invoke(data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Handle Parsed data error");
            }
            return true;
        }

        protected abstract int FindStartIndex();

        protected abstract int FindEndIndex();

        protected FindIndexRsp FindIndex(int startIndex, byte[] block)
        {
            if (block.Length == 0) return new FindIndexRsp { Code = ErrorCode.Success, Index = startIndex };
            if (_bytes.Count - (startIndex - _bytes.StartIndex) < block.Length) return new FindIndexRsp() { Code = ErrorCode.LengthNotEnough, Index = -1 };
            for (int i = startIndex; i < _bytes.StartIndex + _bytes.Count; i++)
            {
                if (_bytes.Bytes[i] == block[0])
                {
                    bool find = true;
                    for (int j = 1; j < block.Length; j++)
                    {
                        if (i + j < _bytes.StartIndex + _bytes.Count)
                        {
                            if (_bytes.Bytes[i + j] != block[j]) { find = false; break; }
                        }
                        else
                        {
                            find = false; break;
                        }
                    }
                    if (find)
                    {
                        return new FindIndexRsp() { Index = i, Code = ErrorCode.Success };
                    }
                }
            }
            return new FindIndexRsp() { Code = ErrorCode.NotFound, Index = -1 };
        }
    }

    public class FindIndexRsp
    {
        public int Index { get; set; }

        public ErrorCode Code { get; set; }
    }

    public enum ErrorCode
    {
        Success,
        LengthNotEnough,
        NotFound
    }
}
