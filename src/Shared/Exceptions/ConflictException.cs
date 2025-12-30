using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Serialization;

namespace Shared.Exceptions;



public class ConflictException : Exception
{
    public string ResourceName {get;}
    public string ConflictField{get;}
    public object ConflictValue{get;}

    public ConflictException(string resourceName,string conflictField, object conflictValue)
    : base($" {resourceName} with {conflictField} '{conflictValue}' already exsists.")
    {
        ResourceName = resourceName;
        ConflictField = conflictField;
        ConflictValue = conflictValue;
    }

    public ConflictException(string message): base(message)
    {
        ResourceName = string.Empty;
        ConflictField = string.Empty;
        ConflictValue = string.Empty;
    }
}