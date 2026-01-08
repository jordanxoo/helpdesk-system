namespace TicketService.Configuration;

public class FileStorageSettings
{
    public string ServiceUrl {get;set;} = string.Empty;
    /// <summary>
    /// Public URL for browser access (e.g., http://localhost:9000)
    /// Used for generating presigned URLs that work from browser
    /// </summary>
    public string PublicServiceUrl {get;set;} = string.Empty;
    public string AccessKey {get;set;} = string.Empty;
    public string SecretKey {get;set;} = string.Empty;
    public string BucketName {get;set;} = "helpdesk-attachments";
}