using BankingApi.DTOs;
using FluentValidation;

namespace BankingApi.Validation;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    private static readonly HashSet<string> SupportedCurrencies = ["EUR", "USD", "GBP", "CHF", "JPY"];

    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.OwnerName)
            .NotEmpty().WithMessage("Owner name is required.")
            .MaximumLength(100).WithMessage("Owner name must not exceed 100 characters.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required.")
            .MaximumLength(50).WithMessage("Owner ID must not exceed 50 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(c => SupportedCurrencies.Contains(c.ToUpperInvariant()))
            .WithMessage($"Currency must be one of: {string.Join(", ", SupportedCurrencies)}.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Initial balance must be non-negative.");
    }
}

public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.OwnerName)
            .NotEmpty().WithMessage("Owner name is required.")
            .MaximumLength(100).WithMessage("Owner name must not exceed 100 characters.");
    }
}

public class CreateCardRequestValidator : AbstractValidator<CreateCardRequest>
{
    public CreateCardRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account ID is required.");

        RuleFor(x => x.CardholderName)
            .NotEmpty().WithMessage("Cardholder name is required.")
            .MaximumLength(100).WithMessage("Cardholder name must not exceed 100 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid card type.");
    }
}

public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    private static readonly HashSet<string> SupportedCurrencies = ["EUR", "USD", "GBP", "CHF", "JPY"];

    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty().WithMessage("Source account ID is required.");

        RuleFor(x => x.ToAccountId)
            .NotEmpty().WithMessage("Destination account ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Must(c => SupportedCurrencies.Contains(c.ToUpperInvariant()))
            .WithMessage($"Currency must be one of: {string.Join(", ", SupportedCurrencies)}.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description must not exceed 250 characters.")
            .When(x => x.Description is not null);
    }
}
