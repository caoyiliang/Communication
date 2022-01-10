namespace TopPortLib
{
    public delegate Task RequestedDataEventHandler(byte[] data);

    public delegate Task RespondedDataEventHandler(byte[] data);

    public delegate Task ReceiveResponseDataEventHandler(Type type, object data);
}
