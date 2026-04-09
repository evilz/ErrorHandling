using BankingApi.Domain.Enums;
using BankingApi.Domain.Models;
using BankingApi.DTOs;
using BankingApi.Infrastructure;
using BankingApi.Repositories;

namespace BankingApi.Services;

public interface ICardService
{
    Task<IReadOnlyList<CardResponse>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CardResponse>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<CardResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CardResponse> CreateAsync(CreateCardRequest request, CancellationToken ct = default);
    Task<CardResponse> ActivateAsync(Guid id, CancellationToken ct = default);
    Task<CardResponse> BlockAsync(Guid id, CancellationToken ct = default);
    Task CancelAsync(Guid id, CancellationToken ct = default);
}

public sealed class CardService(ICardRepository cardRepository, IAccountRepository accountRepository) : ICardService
{
    private static string GenerateMaskedNumber()
    {
        var rng = Random.Shared;
        var last4 = rng.Next(1000, 9999).ToString();
        return $"**** **** **** {last4}";
    }

    private static CardResponse ToResponse(Card c) => new(
        c.Id, c.AccountId, c.MaskedNumber, c.CardholderName,
        c.Type, c.Status, c.ExpiryDate, c.CreatedAt, c.UpdatedAt);

    public async Task<IReadOnlyList<CardResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var cards = await cardRepository.GetAllAsync(ct);
        return cards.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<CardResponse>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
    {
        var cards = await cardRepository.GetByAccountIdAsync(accountId, ct);
        return cards.Select(ToResponse).ToList();
    }

    public async Task<CardResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var card = await cardRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Card '{id}' was not found.");
        return ToResponse(card);
    }

    public async Task<CardResponse> CreateAsync(CreateCardRequest request, CancellationToken ct = default)
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException($"Account '{request.AccountId}' was not found.");

        if (account.Status != AccountStatus.Active)
            throw new BusinessRuleException("Cannot issue a card for a non-active account.");

        var card = new Card
        {
            AccountId = request.AccountId,
            MaskedNumber = GenerateMaskedNumber(),
            CardholderName = request.CardholderName,
            Type = request.Type,
            Status = CardStatus.Pending,
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3))
        };

        var created = await cardRepository.AddAsync(card, ct);
        return ToResponse(created);
    }

    public async Task<CardResponse> ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var card = await cardRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Card '{id}' was not found.");

        if (card.Status == CardStatus.Cancelled || card.Status == CardStatus.Expired)
            throw new BusinessRuleException($"Cannot activate a card with status '{card.Status}'.");

        if (card.Status == CardStatus.Active)
            throw new ConflictException("Card is already active.");

        card.Status = CardStatus.Active;
        card.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await cardRepository.UpdateAsync(card, ct);
        return ToResponse(updated);
    }

    public async Task<CardResponse> BlockAsync(Guid id, CancellationToken ct = default)
    {
        var card = await cardRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Card '{id}' was not found.");

        if (card.Status == CardStatus.Cancelled || card.Status == CardStatus.Expired)
            throw new BusinessRuleException($"Cannot block a card with status '{card.Status}'.");

        if (card.Status == CardStatus.Blocked)
            throw new ConflictException("Card is already blocked.");

        card.Status = CardStatus.Blocked;
        card.UpdatedAt = DateTimeOffset.UtcNow;
        var updated = await cardRepository.UpdateAsync(card, ct);
        return ToResponse(updated);
    }

    public async Task CancelAsync(Guid id, CancellationToken ct = default)
    {
        var card = await cardRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Card '{id}' was not found.");

        if (card.Status == CardStatus.Cancelled)
            throw new ConflictException("Card is already cancelled.");

        card.Status = CardStatus.Cancelled;
        card.UpdatedAt = DateTimeOffset.UtcNow;
        await cardRepository.UpdateAsync(card, ct);
    }
}
