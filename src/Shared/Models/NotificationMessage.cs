namespace Shared.Models;
public record NotificationMessage(
    string id,
    string type,
    string title,
    string body,
    DateTime createdAt,
    Dictionary<string,string>? metadata =null
)
{
    
    public static NotificationMessage Create(
        string type,
        string title,
        string body,
        Dictionary<string,string>? metadata = null
    )
    {
        return new NotificationMessage(
            id: Guid.NewGuid().ToString(),
            type: type,
            title: title,
            body: body,
            createdAt : DateTime.UtcNow,
            metadata: metadata
        );
    }
}