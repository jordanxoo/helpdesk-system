using System.Data;
using FluentValidation;

namespace UserService.Features.Users.Commands.UpdateUser;


public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty()
        .WithMessage("UserId jest wymagany");
        RuleFor(x => x.FirstName)
        .MinimumLength(2).MaximumLength(20)
        .When(x => !string.IsNullOrEmpty(x.FirstName))
        .WithMessage("Imie musi mieć od 2 do 20 znaków");

        RuleFor(x => x.LastName).NotEmpty()
        .When(x => !string.IsNullOrEmpty(x.LastName))
        .MinimumLength(2).MaximumLength(25).WithMessage("Nazwisko musi miec od 2  do 25 znakow");

        RuleFor(x => x.PhoneNumber).Matches(@"^\+?[1-9]\d{8,14}$")
        .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
        .WithMessage("Nieprawidłowy format numeru telefonu (używaj formatu międzynarodowego, min 9 cyfr)");

        RuleFor(x => x.Role)
        .Must(role => role == "Customer" || role == "Agent" || role == "Administrator")
        .When(x => !string.IsNullOrEmpty(x.Role))
        .WithMessage("Rola musi być: Customer, Agent lub Administrator");
    }
}