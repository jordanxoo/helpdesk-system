namespace Shared.DTOs;


public record CreateTicketRequest(
    string Title,
    string Description,
    string Priority,    
    string Category,
    Guid? CustomerId = null,        // Optional - for agents creating tickets on behalf of customers
    Guid? OrganizationId = null     // Optional - manual override by agents/admins
);

public record UpdateTicketRequest(
    string? Title,
    string? Description,
    string? Status,
    string? Priority,
    Guid? AssignedAgentId
);


public record AddCommentRequest(
    string Content,
    bool IsInternal = false
);

public record ChangeStatusRequest(
    string NewStatus
);


public record TicketDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Category,
    Guid CustomerId,
    Guid? AssignedAgentId,
    Guid? OrganizationId,
    Guid? SlaId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ResolvedAt,
    List<CommentDto> Comments
);


public record CommentDto(
    Guid Id,
    Guid UserId,
    string Content,
    DateTime CreatedAt,
    bool IsInternal
);

public record TicketListResponse(
    List<TicketDto> Tickets,
    int TotalCount,
    int Page,
    int PageSize
);


public record TicketFilterRequest(
    string? SearchTerm,
    string? Status,
    string? Priority,
    string? Category,
    Guid? CustomerId,
    Guid? AssignedAgentId,
    int Page = 1,
    int PageSize = 10
);