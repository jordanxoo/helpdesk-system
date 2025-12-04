namespace TicketService.Configuration;

public class FileStorageSettings
{
    public string ServiceUrl {get;set;} = string.Empty;
    public string AccessKey {get;set;} = string.Empty;
    public string SecretKey {get;set;} = string.Empty;
    public string BucketName {get;set;} = "helpdesk-attachments";
}