using FluentValidation;
using Shared.Models;


namespace UserService.Features.Users.Commands.CreateUser;


public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
        .NotEmpty()
        .WithMessage("Imie jest wymagane")
        .MinimumLength(3)
        .MaximumLength(20);

        RuleFor(x => x.LastName).NotEmpty().WithMessage("Nazwisko jest wymagane")
        .MinimumLength(2).MaximumLength(20);

        RuleFor(x => x.Email).NotEmpty().WithMessage("Email jest wymagany").EmailAddress()
        .WithMessage("NIeprawidlowy format Email");

        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Numer telefonu jest wymagany")
        .Matches(@"^\+?[1-9]\d{8,14}$")
        .WithMessage("Nieprawidłowy format numeru telefonu (używaj formatu międzynarodowego, min 9 cyfr)");

        RuleFor(x => x.Role)
        .IsInEnum()
        .WithMessage("Rola musi być: Customer, Agent lub Administrator")
        .Must(role => role == UserRole.Customer || role == UserRole.Agent || role == UserRole.Administrator)
        .WithMessage("Rola musi być: Customer, Agent lub Administrator");

    }
}
