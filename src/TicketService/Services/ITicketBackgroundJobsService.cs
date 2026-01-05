namespace TicketService.Services;

public interface ITicketBackgroundJobsService
{
    //sprawdza tickety przekraczajace SLA >48h bez odp
    Task<int> CheckOverdueSlaTicketsAsync();
    
    // automatycznie zamyka tickety resolved po 7 dniach
    Task<int> AutoCloseResolvedTicketsAsync();
    //wysyla reminder dla ticketow bez odpoweidzi > 24h
    Task<int> SendTicketRemindersAsync();
}