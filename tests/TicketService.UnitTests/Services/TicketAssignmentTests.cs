using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Constants;
using Shared.Events;
using Shared.HttpClients;
using Shared.Messaging;
using Shared.Models;
using TicketService.Repositories;
using TicketService.Services;
using Xunit;

namespace TicketService.UnitTests.Services;

public class TicketAssignmentTests
{
    private readonly TicketServiceImpl _sut; 
    private readonly ITicketRepository _repoMock = Substitute.For<ITicketRepository>();
    private readonly IMessagePublisher _publisherMock = Substitute.For<IMessagePublisher>();
    private readonly IUserServiceClient _userClientMock = Substitute.For<IUserServiceClient>();
    private readonly ILogger<TicketServiceImpl> _loggerMock = Substitute.For<ILogger<TicketServiceImpl>>();

    public TicketAssignmentTests()
    {
        _sut = new TicketServiceImpl(_repoMock, _publisherMock, _userClientMock, _loggerMock);
    }

    [Fact]
    public async Task AssignToAgent_ShouldUpdateStatusAndPublishEvent_WhenTicketExists()
    {
        var ticketId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        
        var existingTicket = new Ticket 
        { 
            Id = ticketId, 
            Status = TicketStatus.New,
            AssignedAgentId = null 
        };

        _repoMock.GetByIdAsync(ticketId, false).Returns(existingTicket);
        
        _repoMock.UpdateAsync(Arg.Any<Ticket>()).Returns(call => call.Arg<Ticket>());

        var result = await _sut.AssignToAgentAsync(ticketId, agentId);

        
        result.Status.Should().Be("Open");
        result.AssignedAgentId.Should().Be(agentId);
        await _repoMock.Received(1).UpdateAsync(Arg.Is<Ticket>(t => 
            t.Status == TicketStatus.Open && 
            t.AssignedAgentId == agentId
        ));

        await _publisherMock.Received(1).PublishAsync(
            Arg.Is<TicketAssignedEvent>(e => e.TicketId == ticketId && e.AgentId == agentId),
            Arg.Is<string>(key => key == RoutingKeys.TicketAssigned)
        );
    }

    [Fact]
    public async Task AssignToAgent_ShouldThrowNotFound_WhenTicketDoesNotExist()
    {
        var ticketId = Guid.NewGuid();
        _repoMock.GetByIdAsync(ticketId, false).Returns((Ticket?)null);

        var action = async () => await _sut.AssignToAgentAsync(ticketId, Guid.NewGuid());

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Ticket with id {ticketId} not found.");
            
        await _publisherMock.DidNotReceive().PublishAsync(Arg.Any<object>(), Arg.Any<string>());
    }
}