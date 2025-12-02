
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shared.DTOs;
using Shared.Models;
using TicketService.Data;

namespace TicketService.IntegrationTests;


public class TicketSearchTests: IClassFixture<TicketServiceFactory>
{
    private readonly TicketServiceFactory _factory;
    private readonly HttpClient _client;


    
    public TicketSearchTests(TicketServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
    }

    [Fact]
    
    public async Task SearchTickets_ShouldReturnOnlyHighPrority_WhenFilteredByHigh()
    {
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
            db.Tickets.RemoveRange(db.Tickets);

            var tickets = new List<Ticket>
            {
                new Ticket { Title = "Critical Bug", Description = "...", Priority = TicketPriority.Critical, Status = TicketStatus.Open, 
                Category = TicketCategory.Software, CreatedAt = DateTime.UtcNow, CustomerId = Guid.NewGuid() },
                new Ticket { Title = "High Issue 1", Description = "...", Priority = TicketPriority.High, Status = TicketStatus.Open, 
                Category = TicketCategory.Hardware, CreatedAt = DateTime.UtcNow, CustomerId = Guid.NewGuid() },
                new Ticket { Title = "High Issue 2", Description = "...", Priority = TicketPriority.High, Status = TicketStatus.InProgress, 
                Category = TicketCategory.Network, CreatedAt = DateTime.UtcNow, CustomerId = Guid.NewGuid() },
                new Ticket { Title = "Low Issue", Description = "...", Priority = TicketPriority.Low, Status = TicketStatus.Open, 
                Category = TicketCategory.Other, CreatedAt = DateTime.UtcNow, CustomerId = Guid.NewGuid() }
            };

            await db.Tickets.AddRangeAsync(tickets);
            await db.SaveChangesAsync();
        }

        var searchRequest = new TicketFilterRequest(SearchTerm: null, Status: null,
        Priority: "High",Category: null, CustomerId: null,AssignedAgentId: null);

        var response = await _client.PostAsJsonAsync("/api/tickets/search",searchRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TicketListResponse>();

        result.Should().NotBe(null);
        result!.TotalCount.Should().Be(2);
        result.Tickets.Should().OnlyContain(t => t.Priority == "High");
        
    }

}