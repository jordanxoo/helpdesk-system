using FluentValidation;
using Shared.DTOs;
using Shared.Models;



namespace TicketService.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Tytuł jest wymagany")
        .MinimumLength(5).WithMessage("Tytuł musi mieć conajmniej 5 znaków")
        .MaximumLength(200).WithMessage("Tytuł nie może być dłuższy niż 200 znaków");

        RuleFor(x => x.Description)
        .NotEmpty().WithMessage("Opis jest wymagany")
        .MinimumLength(20).WithMessage("Opis musi mieć conajmiej 20 znaków");

        RuleFor(x => x.Priority)
        .IsEnumName(typeof(TicketPriority), caseSensitive: false)
        .WithMessage("Nieprawidłowy priorytet. Dostępne: Low, Medium, High");

        RuleFor(x => x.Category)
        .IsEnumName(typeof(TicketCategory),caseSensitive: false)
        .WithMessage("Nieprawidłowa kategoria");

        RuleFor( x => x.CustomerId)
        .Must(id => id != Guid.Empty)
        .When(x => x.CustomerId.HasValue)
        .WithMessage("Nieprawidłowe ID klienta");
    }
}