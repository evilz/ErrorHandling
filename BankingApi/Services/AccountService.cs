using BankingApi.Domain.Enums;
using BankingApi.Domain.Models;
using BankingApi.DTOs;
using BankingApi.Infrastructure;
using BankingApi.Repositories;

namespace BankingApi.Services;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponse>> GetAllAsync(CancellationToken ct = default);
    Task<AccountResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct = default);
    Task<AccountResponse> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default);
    Task CloseAsync(Guid id, CancellationToken ct = default);
    Task<AccountBalanceResponse> GetBalanceAsync(Guid id, CancellationToken ct = default);
}

public sealed class AccountService(IAccountRepository repository) : IAccountService
{
    private static string GenerateAccountNumber() =>
        $"FR{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds():D14}";

    private static AccountResponse ToResponse(Account a) => new(
        a.Id, a.AccountNumber, a.OwnerName, a.OwnerId,
        a.Balance, a.Currency, a.Status, a.CreatedAt, a.UpdatedAt);

    public async Task<IReadOnlyList<AccountResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var accounts = await repository.GetAllAsync(ct);
        return accounts.Select(ToResponse).ToList();
    }

    public async Task<AccountResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var account = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Account '{id}' was not found.");
        return ToResponse(account);
    }

    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(),
            OwnerName = request.OwnerName,
            OwnerId = request.OwnerId,
            Balance = request.InitialBalance,
            Currency = request.Currency,
            Status = AccountStatus.Active
        };
        var created = await repository.AddAsync(account, ct);
        return ToResponse(created);
    }

    public async Task<AccountResponse> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct = default)
    {
        var account = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Account '{id}' was not found.");

        if (account.Status == AccountStatus.Closed)
            throw new BusinessRuleException("Cannot update a closed account.");

        account.OwnerName = request.OwnerName;
        account.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await repository.UpdateAsync(account, ct);
        return ToResponse(updated);
    }

    public async Task CloseAsync(Guid id, CancellationToken ct = default)
    {
        var account = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Account '{id}' was not found.");

        if (account.Status == AccountStatus.Closed)
            throw new ConflictException("Account is already closed.");

        if (account.Balance != 0)
            throw new BusinessRuleException("Cannot close an account with a non-zero balance.");

        account.Status = AccountStatus.Closed;
        account.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.UpdateAsync(account, ct);
    }

    public async Task<AccountBalanceResponse> GetBalanceAsync(Guid id, CancellationToken ct = default)
    {
        var account = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Account '{id}' was not found.");

        return new AccountBalanceResponse(account.Id, account.AccountNumber, account.Balance, account.Currency);
    }
}
