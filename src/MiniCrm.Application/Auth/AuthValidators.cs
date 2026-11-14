
using FluentValidation;

namespace MiniCrm.Application.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);   // bounds the work done hashing the input
    }
}

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Role).Must(r => Roles.All.Contains(r)).WithMessage("Unknown role.");
    }
}