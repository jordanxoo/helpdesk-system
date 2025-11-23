using System.Dynamic;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NSubstitute;
using NSubstitute.Core;
using Shared.DTOs;
using Shared.HttpClients;
using Shared.Messaging;
using Shared.Models;
using TicketService.Repositories;
using TicketService.Services;



namespace TicketService.UnitTests.Services;


public class TicketServiceTests
{
    
    private readonly ITicketRepository _ticketRepoMock = Substitute.For<ITicketRepository>();
    private readonly IMessagePublisher _messagePublisherMock = Substitute.For<IMessagePublisher>();
    private readonly IMessageConsumer _messageConsumerMock = Substitute.For<IMessageConsumer>();
    private readonly IUserServiceClient _userClientMock = Substitute.For<IUserServiceClient>();
    private readonly ILogger<TicketServiceImpl> _loggerMock = Substitute.For<ILogger<TicketServiceImpl>>();

    // sut - system under test
    private readonly TicketServiceImpl _sut; 

    private readonly Faker _faker = new Faker();

    public TicketServiceTests()
    {
        _sut  = new TicketServiceImpl(_ticketRepoMock,
        _messagePublisherMock,_userClientMock,_loggerMock);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTicket_WhenRequestIsValid()
    {
        var userID = Guid.NewGuid();
        var userRole = "Customer";

        var request = new CreateTicketRequest(
            Title: _faker.Lorem.Sentence(),
            Description: _faker.Lorem.Paragraph(),
            Priority: "High",
            Category: "Hardware"
        );

        _ticketRepoMock.CreateAsync(Arg.Any<Ticket>()).Returns(CallInfo =>
        {
            var ticket  = CallInfo.Arg<Ticket>();
            ticket.Id = Guid.NewGuid();
            return ticket;
        });

        var result = await _sut.CreateAsync(userID,userRole,request);

        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Status.Should().Be("New");
        result.CustomerId.Should().Be(userID);
    
    
        await _ticketRepoMock.Received(1).CreateAsync(Arg.Any<Ticket>());

        await _messagePublisherMock.Received(1).PublishAsync(
            Arg.Is<object>(msg => msg.GetType().Name == "TicketCreatedEvent"),
            Arg.Is<string>(key => key == "ticket-created")
        );
    }

    [Fact]

    public async Task CreateAsnyc_ShouldThrowArgumentException_WhenPriorityIsInvalid()
    {
        var request = new CreateTicketRequest("Tytuł", "Opis", "bad_priority","Hardware");

        //funkcja delegat = bo spodziewamy sie ze rzuci wyjatek

        var action = async () => await _sut.CreateAsync(Guid.NewGuid(),"Customer",request);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid priority*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenCategoryIsInvalid()
    {
        var request = new CreateTicketRequest("Tytuł_2","Opis", "High", "labubu");

        var action = async () => await _sut.CreateAsync(Guid.NewGuid(),"Customer",request);


        await action.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid Category*");
    }

    [Fact]

    public async Task CreateAsync_ShouldThrowException_WhenAgentCreatesTicketWithoutCustomerId()
    {
        var request = new CreateTicketRequest("Tytul","Opis","High","Hardware", CustomerId: null);

        var action = async () => await _sut.CreateAsync(Guid.NewGuid(),"Agent",request);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Agents and Administrators must specify CustomerID");
    }




    
    
}