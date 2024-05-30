using System.Runtime.Serialization;

namespace TopPortLib.Exceptions;

/// <summary>Check方法不存在</summary>
public class CheckMethodNotFoundException : Exception
{
    /// <summary>Check方法不存在</summary>
    public CheckMethodNotFoundException() : base() { }
    /// <summary>Check方法不存在</summary>
    public CheckMethodNotFoundException(string message) : base(message) { }
    /// <summary>Check方法不存在</summary>
    public CheckMethodNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
