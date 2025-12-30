namespace Shared.Exceptions;



public class BadRequestException : Exception
{
    public Dictionary<string,string[]>? Errors{get;}

    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Dictionary<string,string[]> dict)
    :base(message)
    {
        Errors = dict;
    }
}