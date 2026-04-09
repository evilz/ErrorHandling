using BankingApi.Domain.Enums;

namespace BankingApi.DTOs;

// ─── Card DTOs ─────────────────────────────────────────────────────────────────

public record CreateCardRequest(
    Guid AccountId,
    string CardholderName,
    CardType Type
);

public record CardResponse(
    Guid Id,
    Guid AccountId,
    string MaskedNumber,
    string CardholderName,
    CardType Type,
    CardStatus Status,
    DateOnly ExpiryDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
