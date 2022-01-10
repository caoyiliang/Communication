namespace Crow
{
    public delegate Task SentDataEventHandler<T>(T data);

    public delegate Task ReceivedDataEventHandler<T>(T data);
}
