using BankingApi.Domain.Models;

namespace BankingApi.Repositories;

public interface IAccountRepository
{
    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct = default);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task<Account> AddAsync(Account account, CancellationToken ct = default);
    Task<Account> UpdateAsync(Account account, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
