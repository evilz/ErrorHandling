using BankingApi.Domain.Enums;

namespace BankingApi.Domain.Models;

public class Card
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid AccountId { get; init; }
    public string MaskedNumber { get; init; } = string.Empty;
    public CardType Type { get; init; }
    public CardStatus Status { get; set; } = CardStatus.Pending;
    public string CardholderName { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
