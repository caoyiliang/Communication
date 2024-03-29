﻿namespace Parser.Timers
{
    internal class NormalTimer : ITimer
    {
        private readonly AutoResetEvent _autoResetEvent = new(false);
        public void Release()
        {
            _autoResetEvent.Set();
        }

        public bool Wait(int timeout)
        {
            return _autoResetEvent.WaitOne(timeout);
        }
    }
}
