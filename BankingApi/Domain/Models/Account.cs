using BankingApi.Domain.Enums;

namespace BankingApi.Domain.Models;

public class Account
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string AccountNumber { get; init; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerId { get; init; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; init; } = "EUR";
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
