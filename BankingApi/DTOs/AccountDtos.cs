using BankingApi.Domain.Enums;

namespace BankingApi.DTOs;

// ─── Account DTOs ──────────────────────────────────────────────────────────────

public record CreateAccountRequest(
    string OwnerName,
    string OwnerId,
    string Currency = "EUR",
    decimal InitialBalance = 0m
);

public record UpdateAccountRequest(
    string OwnerName
);

public record AccountResponse(
    Guid Id,
    string AccountNumber,
    string OwnerName,
    string OwnerId,
    decimal Balance,
    string Currency,
    AccountStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record AccountBalanceResponse(
    Guid AccountId,
    string AccountNumber,
    decimal Balance,
    string Currency
);
