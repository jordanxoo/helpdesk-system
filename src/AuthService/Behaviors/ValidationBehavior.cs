using FluentValidation;
using MediatR;



namespace AuthService.Behaviors;


public class ValidationBehavior<TRequest,TResponse> : IPipelineBehavior<TRequest,TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validator)
    {
        _validators = validator;
    }

     /// <summary>
    /// Handle() = główna metoda behaviora
    /// Wywołana przez MediatR dla KAŻDEGO requestu typu TRequest
    /// </summary>
    /// <param name="request">Command/Query do zwalidowania</param>
    /// <param name="next">Delegat do kolejnego behaviora/handlera (jak next() w Express.js)</param>
    /// <param name="cancellationToken">Token do anulowania</param>
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    )
    {
        Console.WriteLine($"[ValidationBehavior] Handling request of type: {typeof(TRequest).Name}");
        Console.WriteLine($"[ValidationBehavior] Found {_validators.Count()} validators");
        
        if (!_validators.Any())
        {
            //brak walidatorow - skip walidacji przechodzimy dalej
            // GetPasswordRequirements nei ma validatora (publiczny endpoint)
            Console.WriteLine($"[ValidationBehavior] No validators found, skipping validation");
            return await next();
        }

        //stworzenie context dla fluentvalidation 
        var context = new ValidationContext<TRequest>(request);


        //zebranie wszystkich bledow od validatorow
        // wszystkie validatory wykonywane rownolegle -> Task.whenall <-
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context,ct))
        );

        var failures = validationResults.Where(r => r.Errors.Any())
        .SelectMany(r => r.Errors)
        .ToList();

        if(failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();


    }    
}