using System.Collections.Concurrent;
using BankingApi.Domain.Models;
using BankingApi.Repositories;

namespace BankingApi.Infrastructure;

public sealed class InMemoryCardRepository : ICardRepository
{
    private readonly ConcurrentDictionary<Guid, Card> _store = new();

    public Task<IReadOnlyList<Card>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Card>>(_store.Values.OrderBy(c => c.CreatedAt).ToList());

    public Task<IReadOnlyList<Card>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Card>>(_store.Values
            .Where(c => c.AccountId == accountId)
            .OrderBy(c => c.CreatedAt)
            .ToList());

    public Task<Card?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<Card> AddAsync(Card card, CancellationToken ct = default)
    {
        _store[card.Id] = card;
        return Task.FromResult(card);
    }

    public Task<Card> UpdateAsync(Card card, CancellationToken ct = default)
    {
        _store[card.Id] = card;
        return Task.FromResult(card);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.TryRemove(id, out _));
}
