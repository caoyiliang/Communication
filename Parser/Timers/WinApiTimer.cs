using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Timers
{
    internal class WinApiTimer : ITimer
    {
        /// <summary>
        /// 计时器分辨率的信息
        /// </summary>
        private static TIMECAPS timecaps;

        /// <summary>
        ///作为fptc参数的函数指针
        /// </summary>
        private TimerExtCallback timerExtCallback;

        TaskCompletionSource<bool> taskCompletionSource;
        uint result;
        uint timeout;

        #region  扩展

        private delegate void TimerExtCallback(uint uTimerID, uint uMsg, uint dwUser, UIntPtr dw1, UIntPtr dw2); // timeSetEvent所对应的回调函数的签名

        /// <summary>
        /// 查询计时器设备以确定其分辨率成功
        /// </summary>
        private const int TIMERR_NOERROR = 0x0000;

        /// <summary>
        /// 当计时器到期时，系统将调用fptc参数指向的函数。
        /// </summary>
        private const int TIME_CALLBACK_FUNCTION = 0x0001;

        /// <summary>
        /// 此结构包含有关计时器分辨率的信息。单位是ms
        /// </summary>
        [Description("此结构包含有关计时器分辨率的信息。单位是ms")]
        [StructLayout(LayoutKind.Sequential)]
        public struct TIMECAPS
        {
            /// <summary>
            /// 支持的最小期限。
            /// </summary>
            [Description("支持的最小期限")]
            public uint wPeriodMin;
            /// <summary>
            /// 支持的最大期限。
            /// </summary>
            [Description("支持的最大期限")]
            public uint wPeriodMax;
        }

        /// <summary>
        /// 此函数启动指定的计时器事件。
        /// </summary>
        /// <param name="uDelay">事件延迟，以毫秒为单位。如果该值不在计时器支持的最小和最大事件延迟范围内，则该函数返回错误。</param>
        /// <param name="uResolution">计时器事件的分辨率，以毫秒为单位。分辨率越高，分辨率越高；零分辨率表示周期性事件应该以最大可能的精度发生。但是，为减少系统开销，应使用适合您的应用程序的最大值。</param>
        /// <param name="fptc">如果fuEvent指定TIME_CALLBACK_EVENT_SET或TIME_CALLBACK_EVENT_PULSE标志，则fptc参数将解释为事件对象的句柄。事件将在单个事件完成时设置或发出脉冲，或者在周期性事件完成时定期设置或触发。对于fuEvent的任何其他值，fptc参数将被解释为函数指针。</param>
        /// <param name="dwUser">用户提供的回调数据。</param>
        /// <param name="fuEvent">计时器事件类型。下表显示了fuEvent参数可以包含的值。</param>
        /// <returns></returns>
        [DllImport("winmm.dll")]
        private static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerExtCallback fptc, uint dwUser, uint fuEvent);

        /// <summary>
        /// 此功能取消指定的计时器事件。
        /// </summary>
        /// <param name="id">要取消的计时器事件的标识符。此标识符由timeSetEvent函数返回，该函数启动指定的计时器事件。</param>
        /// <returns></returns>
        [DllImport("winmm.dll")]
        private static extern uint timeKillEvent(uint uTimerID);

        /// <summary>
        /// 此函数查询计时器设备以确定其分辨率。
        /// </summary>
        /// <param name="ptc">指向TIMECAPS结构的指针。该结构充满了有关计时器设备分辨率的信息。</param>
        /// <param name="cbtc">TIMECAPS结构的大小（以字节为单位）。</param>
        /// <returns>如果成功，则返回TIMERR_NOERROR，如果未能返回计时器设备功能，则返回TIMERR_STRUCT。</returns>
        [DllImport("winmm.dll")]
        private static extern uint timeGetDevCaps(ref TIMECAPS ptc, int cbtc);

        #endregion

        static WinApiTimer()
        {
            uint result = timeGetDevCaps(ref timecaps, Marshal.SizeOf(timecaps));
            if (result != TIMERR_NOERROR)
            {
                throw new Exception("timeGetDevCaps失败");
            }
        }

        public WinApiTimer()
        {
            this.timerExtCallback = new TimerExtCallback(this.TimerExtCallbackFun);
        }

        public void Release()
        {
            timeKillEvent(result);
            result = timeSetEvent(timeout, Math.Min(1, timecaps.wPeriodMin), this.timerExtCallback, 0, TIME_CALLBACK_FUNCTION); // 间隔性地运行
            if (result == 0)
            {
                throw new Exception("timeSetEvent启动计时器失败");
            }
        }

        public bool Wait(int timeout)
        {
            taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            this.timeout = (uint)timeout;
            uint result = timeSetEvent((uint)timeout, Math.Min(1, timecaps.wPeriodMin), this.timerExtCallback, 0, TIME_CALLBACK_FUNCTION); // 间隔性地运行
            if (result == 0)
            {
                throw new Exception("timeSetEvent启动计时器失败");
            }
            while (!taskCompletionSource.Task.IsCompleted)
            {

            }
            timeKillEvent(result);
            return false;
        }

        private void TimerExtCallbackFun(uint uTimerID, uint uMsg, uint dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            taskCompletionSource?.TrySetResult(true);
        }
    }
}
