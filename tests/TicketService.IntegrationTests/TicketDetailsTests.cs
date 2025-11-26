
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Bcpg;
using Shared.DTOs;
using Shared.Models;
using TicketService.Data;
using TicketService.Services;

namespace TicketService.IntegrationTests;

public class TicketDetailsTests : IClassFixture<TicketServiceFactory>
{
    private readonly TicketServiceFactory _factory;
    private readonly HttpClient _client;

    public TicketDetailsTests(TicketServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]

    public async Task GetById_ShouldReturnTicketWithComments_WhenTheyExsists()
    {
        var ticketID = Guid.NewGuid();
        var userID = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = ticketID,
            Title = "Test Ticket with comments",
            Description = "Description",
            Status = TicketStatus.New,
            Priority = TicketPriority.High,
            Category = TicketCategory.Hardware,
            CustomerId = userID,
            CreatedAt = DateTime.UtcNow
        };

        var comments = new List<TicketComment>
        {
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketID,
                UserId = userID,
                Content = "First comment",
                CreatedAt = DateTime.UtcNow.AddMinutes(5),
                IsInternal = false
            },
            new TicketComment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketID,
                UserId = Guid.NewGuid(),
                Content = "Agent reply",
                CreatedAt = DateTime.UtcNow.AddMinutes(10),
                IsInternal = false
            }

        };

        using(var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            await db.Tickets.AddAsync(ticket);
            await db.TicketComments.AddRangeAsync(comments);
            await db.SaveChangesAsync();

        };

        var response = await _client.GetAsync($"/api/tickets/{ticketID}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);


        var resultDTO = await response.Content.ReadFromJsonAsync<TicketDto>();

        resultDTO.Should().NotBe(null);
        resultDTO!.Id.Should().Be(ticketID);
        resultDTO.Title.Should().Be("Test Ticket with comments");
    
        resultDTO.Comments[0].Should().NotBe(null);
        resultDTO.Comments[1].Should().NotBe(null);
        resultDTO.Comments.Should().HaveCount(2);
    
        resultDTO.Comments[0].Content.Should().Be("First comment");
        resultDTO.Comments[1].Content.Should().Be("Agent reply");
    }

}