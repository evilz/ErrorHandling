using BankingApi.Domain.Models;

namespace BankingApi.Repositories;

public interface ICardRepository
{
    Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Card>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Card> AddAsync(Card card, CancellationToken ct = default);
    Task<Card> UpdateAsync(Card card, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
