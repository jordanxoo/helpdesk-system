using MediatR;
using KnowledgeBaseService.DTOs;


namespace KnowledgeBaseService.Features.Articles.Commands;


public record CreateArticleCommand(
    string Title,
    string Content,
    string? Summary,
    Guid? CategoryId,
    List<Guid> TagIds,
    Guid AuthorId,
    string AuthorName,
    string Status
) : IRequest<ArticleDto>;