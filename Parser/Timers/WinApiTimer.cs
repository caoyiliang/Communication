using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Parser.Timers
{
    internal partial class WinApiTimer : ITimer
    {
        /// <summary>
        /// 计时器分辨率的信息
        /// </summary>
        private static TIMECAPS timecaps;

        /// <summary>
        ///作为fptc参数的函数指针
        /// </summary>
        private readonly TimerExtCallback timerExtCallback;

        TaskCompletionSource<bool>? taskCompletionSource;
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

        #endregion

        static WinApiTimer()
        {
            uint result = timeGetDevCaps(ref timecaps, Marshal.SizeOf(timecaps));
            if (result != TIMERR_NOERROR)
            {
                throw new Exception("timeGetDevCaps失败");
            }
        }

        public WinApiTimer() => this.timerExtCallback = new TimerExtCallback(this.TimerExtCallbackFun);

        public void Release()
        {
            _ = timeKillEvent(result);
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
            _ = timeKillEvent(result);
            return false;
        }

        private void TimerExtCallbackFun(uint uTimerID, uint uMsg, uint dwUser, UIntPtr dw1, UIntPtr dw2)
        {
            taskCompletionSource?.TrySetResult(true);
        }
    }
}
