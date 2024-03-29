﻿using LogInterface;
using Parser.Interfaces;

namespace Parser.Parsers
{
    /// <summary>
    /// 无解析器
    /// </summary>
    public class NoParser : IParser
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<NoParser>();
        /// <inheritdoc/>
        public event ReceiveParsedDataEventHandler? OnReceiveParsedData;

        /// <inheritdoc/>
        public async Task ReceiveOriginalDataAsync(byte[] data, int size)
        {
            var bytes = new byte[size];
            Array.Copy(data, bytes, size);
            if (OnReceiveParsedData != null)
            {
                try
                {
                    await OnReceiveParsedData(bytes);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Handle Parsed data error");
                }
            }
        }
    }
}
