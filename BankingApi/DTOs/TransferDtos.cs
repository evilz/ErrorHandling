using BankingApi.Domain.Enums;

namespace BankingApi.DTOs;

// ─── Transfer DTOs ─────────────────────────────────────────────────────────────

public record CreateTransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency = "EUR",
    string? Description = null
);

public record TransferResponse(
    Guid Id,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    TransferStatus Status,
    string Reference,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt
);
