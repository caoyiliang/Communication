/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：TilesSendException.cs
********************************************************************/

namespace Crow.Exceptions
{
    /// <summary>
    /// 瓦片发送异常
    /// </summary>
    public class TilesSendException : Exception
    {
        public TilesSendException() : base() { }

        public TilesSendException(string message) : base(message) { }

        public TilesSendException(string message, Exception innerException) : base(message, innerException) { }
    }
}
