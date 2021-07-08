/********************************************************************
 * * 作者： 曹一梁 周俊峰
 * * 说明：ITimer.cs
********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Timers
{
    internal interface ITimer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>false:超时</returns>
        bool Wait(int timeout);

        void Release();
    }
}
