namespace MarkdownPlus.Core.Exceptions;

public class InvalidCacheException : Exception
{
    public  InvalidCacheException() : base() { }
    public  InvalidCacheException(string msg) : base(msg) { }
}
