namespace Communication.Exceptions
{
    public class ConnectFailedException : Exception
    {
        public ConnectFailedException() : base() { }

        public ConnectFailedException(string message) : base(message) { }

        public ConnectFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
