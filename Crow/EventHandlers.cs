/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：EventHandlers.cs
********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Crow
{
    public delegate Task SentDataEventHandler<T>(T data);

    public delegate Task ReceivedDataEventHandler<T>(T data);
}
