using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Parser.Timers
{
    internal partial class WinApiTimer
    {
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
        /// <param name="uTimerID">要取消的计时器事件的标识符。此标识符由timeSetEvent函数返回，该函数启动指定的计时器事件。</param>
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
    }
}
