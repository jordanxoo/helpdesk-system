using Amazon.Runtime.Internal.Util;
using NSubstitute;
using Shared.HttpClients;
using Shared.Messaging;
using TicketService.Repositories;
using TicketService.Services;
using Microsoft.Extensions.Logging;
using Shared.Models;
using FluentAssertions;
using Shared.Events;
using Shared.Constants;
namespace TicketService.UnitTests.Services;


public class TicketStatusTests
{
    private readonly TicketServiceImpl _sut;
    
    private readonly ITicketRepository _repositoryMock = Substitute.For<ITicketRepository>();
    private readonly IMessagePublisher _publisherMock = Substitute.For<IMessagePublisher>();

    private readonly IUserServiceClient _userServiceMock = Substitute.For<IUserServiceClient>();
    private readonly ILogger<TicketServiceImpl> _loggerMock = Substitute.For<ILogger<TicketServiceImpl>>();

    public TicketStatusTests()
    {
        _sut = new TicketServiceImpl(_repositoryMock,_publisherMock,_userServiceMock,_loggerMock);
    }

    [Fact]

    public async Task ChangeStatus_ShouldUpdateStatus_And_PublishEvent()
    {
        var ticketId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var exsistingTicket = new Ticket
        {
            Id = ticketId,
            Title = "Test ticket",
            Status = TicketStatus.New,
            CustomerId = customerId
        };

        _repositoryMock.GetByIdAsync(ticketId,false).Returns(exsistingTicket);

        _repositoryMock.UpdateAsync(Arg.Any<Ticket>()).Returns(call => call.Arg<Ticket>());

        var result = await _sut.ChangeStatusAsync(ticketId,"InProgress");

        result.Status.Should().Be("InProgress");


        await _repositoryMock.Received(1).UpdateAsync(Arg.Is<Ticket>(t => t.Status == TicketStatus.InProgress));

        await _publisherMock.Received(1).PublishAsync(Arg.Is<TicketStatusChangedEvent>( e => 
        e.TicketId == ticketId &&
        e.OldStatus == "New" && 
        e.NewStatus == "InProgress"),Arg.Is<string>(key => key == RoutingKeys.TicketStatusChanged));
    }
}
