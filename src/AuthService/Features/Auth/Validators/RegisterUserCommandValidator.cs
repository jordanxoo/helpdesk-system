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
        .MinimumLength(6).WithMessage("Haslo musi miec przynajmniej 6 znakow");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => role == "Customer" || role == "Agent" || role == "Administrator")
            .WithMessage("Nieprawidłowa rola");
        }
}