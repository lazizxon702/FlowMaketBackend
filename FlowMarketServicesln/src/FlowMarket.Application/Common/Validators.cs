using FluentValidation;
using FlowMarket.Application.Auth;
using FlowMarket.Application.Products;

namespace FlowMarket.Application.Common;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).MinimumLength(10);
    }
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).NotEmpty();
    }
}

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Request.Description).MaximumLength(2000);
        RuleFor(x => x.Request.Price).GreaterThan(0);
        RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
