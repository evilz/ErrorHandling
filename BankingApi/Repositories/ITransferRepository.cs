using BankingApi.Domain.Models;

namespace BankingApi.Repositories;

public interface ITransferRepository
{
    Task<IReadOnlyList<Transfer>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Transfer>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<Transfer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Transfer> AddAsync(Transfer transfer, CancellationToken ct = default);
    Task<Transfer> UpdateAsync(Transfer transfer, CancellationToken ct = default);
}
