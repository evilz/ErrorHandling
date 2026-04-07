using BankingApi.Domain.Enums;

namespace BankingApi.Domain.Models;

public class Transfer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "EUR";
    public TransferStatus Status { get; set; } = TransferStatus.Pending;
    public string Reference { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}
