using BankingApi.Domain.Enums;
using BankingApi.Domain.Models;
using BankingApi.DTOs;
using BankingApi.Infrastructure;
using BankingApi.Repositories;

namespace BankingApi.Services;

public interface ITransferService
{
    Task<IReadOnlyList<TransferResponse>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TransferResponse>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<TransferResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TransferResponse> CreateAsync(CreateTransferRequest request, CancellationToken ct = default);
}

public sealed class TransferService(
    ITransferRepository transferRepository,
    IAccountRepository accountRepository) : ITransferService
{
    // Global lock ensures atomicity of the debit + credit operation.
    // A production system would use database transactions or optimistic concurrency instead.
    private static readonly SemaphoreSlim _transferLock = new(1, 1);

    private static string GenerateReference() =>
        $"TRF-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    private static TransferResponse ToResponse(Transfer t) => new(
        t.Id, t.FromAccountId, t.ToAccountId,
        t.Amount, t.Currency, t.Status,
        t.Reference, t.Description, t.CreatedAt, t.CompletedAt);

    public async Task<IReadOnlyList<TransferResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var transfers = await transferRepository.GetAllAsync(ct);
        return transfers.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<TransferResponse>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        var transfers = await transferRepository.GetByAccountIdAsync(accountId, ct);
        return transfers.Select(ToResponse).ToList();
    }

    public async Task<TransferResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var transfer = await transferRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Transfer '{id}' was not found.");
        return ToResponse(transfer);
    }

    public async Task<TransferResponse> CreateAsync(CreateTransferRequest request, CancellationToken ct = default)
    {
        if (request.FromAccountId == request.ToAccountId)
            throw new BusinessRuleException("Source and destination accounts must be different.");

        if (request.Amount <= 0)
            throw new BusinessRuleException("Transfer amount must be greater than zero.");

        // Validate accounts before acquiring the lock to fail fast without holding it.
        var fromAccount = await accountRepository.GetByIdAsync(request.FromAccountId, ct)
            ?? throw new NotFoundException($"Source account '{request.FromAccountId}' was not found.");

        var toAccount = await accountRepository.GetByIdAsync(request.ToAccountId, ct)
            ?? throw new NotFoundException($"Destination account '{request.ToAccountId}' was not found.");

        if (fromAccount.Status != AccountStatus.Active)
            throw new BusinessRuleException("Source account is not active.");

        if (toAccount.Status != AccountStatus.Active)
            throw new BusinessRuleException("Destination account is not active.");

        if (fromAccount.Currency != request.Currency)
            throw new BusinessRuleException($"Source account currency '{fromAccount.Currency}' does not match transfer currency '{request.Currency}'.");

        await _transferLock.WaitAsync(ct);
        try
        {
            // Re-read accounts inside the lock to get the latest balances.
            fromAccount = await accountRepository.GetByIdAsync(request.FromAccountId, ct)
                ?? throw new NotFoundException($"Source account '{request.FromAccountId}' was not found.");

            toAccount = await accountRepository.GetByIdAsync(request.ToAccountId, ct)
                ?? throw new NotFoundException($"Destination account '{request.ToAccountId}' was not found.");

            if (fromAccount.Balance < request.Amount)
                throw new BusinessRuleException("Insufficient funds in source account.");

            var transfer = new Transfer
            {
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                Reference = GenerateReference(),
                Status = TransferStatus.Pending
            };

            await transferRepository.AddAsync(transfer, ct);

            fromAccount.Balance -= request.Amount;
            fromAccount.UpdatedAt = DateTimeOffset.UtcNow;
            await accountRepository.UpdateAsync(fromAccount, ct);

            toAccount.Balance += request.Amount;
            toAccount.UpdatedAt = DateTimeOffset.UtcNow;
            await accountRepository.UpdateAsync(toAccount, ct);

            transfer.Status = TransferStatus.Completed;
            transfer.CompletedAt = DateTimeOffset.UtcNow;
            var completed = await transferRepository.UpdateAsync(transfer, ct);

            return ToResponse(completed);
        }
        finally
        {
            _transferLock.Release();
        }
    }
}
