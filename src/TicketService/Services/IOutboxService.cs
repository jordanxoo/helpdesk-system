namespace TicketService.Services;



public interface IOutboxService
{
    ///<summary>
    /// zapisuje event do outbox table w tej samej transakcji co business data
    /// generic parameter - generacja specific method dla kazdego typu
    /// </summary>
    
    Task SaveEventAsync<TEvent>(TEvent @event,Guid aggregateId) where TEvent : class;
    //task zwaraca promise-like object(await able)
}