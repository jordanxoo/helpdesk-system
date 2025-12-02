using FluentAssertions;
using NetArchTest.Rules;
using Shared.Models;
using TicketService.Controllers;
using TicketService.Repositories;

namespace TicketService.UnitTests;


public class ArchitectureTests
{
    [Fact]
    public void Controllers_Should_Not_Depend_On_DbContext_Directly()
    {
        var assembly = typeof(TicketsController).Assembly;

         var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace("TicketService.Controllers")
            .ShouldNot()
            .HaveDependencyOn("TIcketService.Data")
            .GetResult();
    
        Assert.True(result.IsSuccessful,"Controllers should not access data layer directly");
    }

    [Fact]

    public void Repositories_Should_Have_Repository_Suffix()
    {
        var assembly = typeof(TicketRepository).Assembly;

        var result = Types.InAssembly(assembly)
        .That()
        .ImplementInterface(typeof(ITicketRepository))
        .Should()
        .HaveNameEndingWith("Repository")
        .GetResult();

        Assert.True(result.IsSuccessful);
    }
}