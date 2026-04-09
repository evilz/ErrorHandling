using System.Collections.Concurrent;
using BankingApi.Domain.Models;
using BankingApi.Repositories;

namespace BankingApi.Infrastructure;

public sealed class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<Guid, Account> _store = new();

    public Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Account>>(_store.Values.OrderBy(a => a.CreatedAt).ToList());

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.GetValueOrDefault(id));

    public Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default) =>
        Task.FromResult(_store.Values.FirstOrDefault(a => a.AccountNumber == accountNumber));

    public Task<Account> AddAsync(Account account, CancellationToken ct = default)
    {
        _store[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task<Account> UpdateAsync(Account account, CancellationToken ct = default)
    {
        _store[account.Id] = account;
        return Task.FromResult(account);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.TryRemove(id, out _));
}
