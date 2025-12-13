using FluentValidation;

namespace AuthService.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email jest wymagany")
            .EmailAddress().WithMessage("Nieprawidłowy format email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Hasło jest wymagane")
            .MinimumLength(6).WithMessage("Hasło musi mieć przynajmniej 6 znaków");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Imię jest wymagane");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Nazwisko jest wymagane");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Rola jest wymagana")
            .Must(role => role == "Customer" || role == "Agent" || role == "Administrator")
            .WithMessage("Nieprawidłowa rola");
    }
}
