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
