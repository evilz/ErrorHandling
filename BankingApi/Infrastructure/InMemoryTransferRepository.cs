using System.Collections.Concurrent;
using BankingApi.Domain.Models;
using BankingApi.Repositories;

namespace BankingApi.Infrastructure;

public sealed class InMemoryTransferRepository : ITransferRepository
{
    private readonly ConcurrentDictionary<Guid, Transfer> _store = new();

    public Task<IReadOnlyList<Transfer>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Transfer>>(_store.Values.OrderByDescending(t => t.CreatedAt).ToList());

    public Task<IReadOnlyList<Transfer>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Transfer>>(_store.Values
            .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToList());

    public Task<Transfer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<Transfer> AddAsync(Transfer transfer, CancellationToken ct = default)
    {
        _store[transfer.Id] = transfer;
        return Task.FromResult(transfer);
    }

    public Task<Transfer> UpdateAsync(Transfer transfer, CancellationToken ct = default)
    {
        _store[transfer.Id] = transfer;
        return Task.FromResult(transfer);
    }
}
