using System.Net;
using System.Net.Http.Json;
using BankingApi.Domain.Enums;
using BankingApi.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankingApi.Tests;

public class CardsIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly AccountsIntegrationTests _accountHelper = new(factory);

    [Fact]
    public async Task GetAllCards_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/cards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cards = await response.Content.ReadFromJsonAsync<List<CardResponse>>();
        Assert.NotNull(cards);
    }

    [Fact]
    public async Task CreateCard_WithValidAccount_ReturnsCreated()
    {
        var account = await _accountHelper.CreateTestAccountAsync("Grace Kelly", "owner-card-001", 1000m);
        var request = new CreateCardRequest(account.Id, "Grace Kelly", CardType.Debit);

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var card = await response.Content.ReadFromJsonAsync<CardResponse>();
        Assert.NotNull(card);
        Assert.Equal(account.Id, card.AccountId);
        Assert.Equal(CardType.Debit, card.Type);
        Assert.Equal(CardStatus.Pending, card.Status);
        Assert.Equal("Grace Kelly", card.CardholderName);
    }

    [Fact]
    public async Task CreateCard_WithInvalidAccount_Returns404()
    {
        var request = new CreateCardRequest(Guid.NewGuid(), "Ghost User", CardType.Debit);

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ActivateCard_FromPending_ReturnsActiveCard()
    {
        var account = await _accountHelper.CreateTestAccountAsync("Henry Adams", "owner-card-002", 0m);
        var card = await CreateTestCardAsync(account.Id, "Henry Adams", CardType.Credit);

        var response = await _client.PutAsync($"/api/cards/{card.Id}/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var activated = await response.Content.ReadFromJsonAsync<CardResponse>();
        Assert.NotNull(activated);
        Assert.Equal(CardStatus.Active, activated.Status);
    }

    [Fact]
    public async Task BlockCard_FromActive_ReturnsBlockedCard()
    {
        var account = await _accountHelper.CreateTestAccountAsync("Iris Fox", "owner-card-003", 0m);
        var card = await CreateTestCardAsync(account.Id, "Iris Fox", CardType.Debit);
        await _client.PutAsync($"/api/cards/{card.Id}/activate", null);

        var response = await _client.PutAsync($"/api/cards/{card.Id}/block", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var blocked = await response.Content.ReadFromJsonAsync<CardResponse>();
        Assert.NotNull(blocked);
        Assert.Equal(CardStatus.Blocked, blocked.Status);
    }

    [Fact]
    public async Task CancelCard_ReturnsNoContent()
    {
        var account = await _accountHelper.CreateTestAccountAsync("Jack Noir", "owner-card-004", 0m);
        var card = await CreateTestCardAsync(account.Id, "Jack Noir", CardType.Debit);

        var response = await _client.DeleteAsync($"/api/cards/{card.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetCardById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/cards/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<CardResponse> CreateTestCardAsync(
        Guid accountId, string cardholderName, CardType type)
    {
        var request = new CreateCardRequest(accountId, cardholderName, type);
        var response = await _client.PostAsJsonAsync("/api/cards", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CardResponse>())!;
    }
}
