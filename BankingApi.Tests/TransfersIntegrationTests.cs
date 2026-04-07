using System.Net;
using System.Net.Http.Json;
using BankingApi.Domain.Enums;
using BankingApi.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankingApi.Tests;

public class TransfersIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly AccountsIntegrationTests _accountHelper = new(factory);

    [Fact]
    public async Task GetAllTransfers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/transfers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transfers = await response.Content.ReadFromJsonAsync<List<TransferResponse>>();
        Assert.NotNull(transfers);
    }

    [Fact]
    public async Task CreateTransfer_BetweenValidAccounts_ReturnsCreated()
    {
        var from = await _accountHelper.CreateTestAccountAsync("Kyle Sender", "owner-tr-001", 1000m);
        var to = await _accountHelper.CreateTestAccountAsync("Lisa Receiver", "owner-tr-002", 0m);
        var request = new CreateTransferRequest(from.Id, to.Id, 200m, "EUR", "Payment for services");

        var response = await _client.PostAsJsonAsync("/api/transfers", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var transfer = await response.Content.ReadFromJsonAsync<TransferResponse>();
        Assert.NotNull(transfer);
        Assert.Equal(200m, transfer.Amount);
        Assert.Equal(TransferStatus.Completed, transfer.Status);
        Assert.NotNull(transfer.CompletedAt);
        Assert.NotEmpty(transfer.Reference);
    }

    [Fact]
    public async Task CreateTransfer_DeductsFromSourceAndCreditsDestination()
    {
        var from = await _accountHelper.CreateTestAccountAsync("Mike Pays", "owner-tr-003", 500m);
        var to = await _accountHelper.CreateTestAccountAsync("Nancy Gets", "owner-tr-004", 100m);
        var request = new CreateTransferRequest(from.Id, to.Id, 300m, "EUR");

        await _client.PostAsJsonAsync("/api/transfers", request);

        var fromBalance = await _client.GetFromJsonAsync<AccountBalanceResponse>($"/api/accounts/{from.Id}/balance");
        var toBalance = await _client.GetFromJsonAsync<AccountBalanceResponse>($"/api/accounts/{to.Id}/balance");

        Assert.NotNull(fromBalance);
        Assert.NotNull(toBalance);
        Assert.Equal(200m, fromBalance.Balance);
        Assert.Equal(400m, toBalance.Balance);
    }

    [Fact]
    public async Task CreateTransfer_InsufficientFunds_Returns422()
    {
        var from = await _accountHelper.CreateTestAccountAsync("Oscar Poor", "owner-tr-005", 50m);
        var to = await _accountHelper.CreateTestAccountAsync("Penny Rich", "owner-tr-006", 0m);
        var request = new CreateTransferRequest(from.Id, to.Id, 500m, "EUR");

        var response = await _client.PostAsJsonAsync("/api/transfers", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransfer_SameAccount_Returns422()
    {
        var account = await _accountHelper.CreateTestAccountAsync("Quinn Self", "owner-tr-007", 1000m);
        var request = new CreateTransferRequest(account.Id, account.Id, 100m, "EUR");

        var response = await _client.PostAsJsonAsync("/api/transfers", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransfer_WithNegativeAmount_ReturnsBadRequest()
    {
        var from = await _accountHelper.CreateTestAccountAsync("Ralph Neg", "owner-tr-008", 1000m);
        var to = await _accountHelper.CreateTestAccountAsync("Sara Neg", "owner-tr-009", 0m);
        var request = new CreateTransferRequest(from.Id, to.Id, -50m, "EUR");

        var response = await _client.PostAsJsonAsync("/api/transfers", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTransferById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/transfers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountTransfers_ReturnsTransfersForAccount()
    {
        var from = await _accountHelper.CreateTestAccountAsync("Tom Gives", "owner-tr-010", 1000m);
        var to = await _accountHelper.CreateTestAccountAsync("Uma Takes", "owner-tr-011", 0m);
        var request = new CreateTransferRequest(from.Id, to.Id, 100m, "EUR");
        await _client.PostAsJsonAsync("/api/transfers", request);

        var response = await _client.GetAsync($"/api/accounts/{from.Id}/transfers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transfers = await response.Content.ReadFromJsonAsync<List<TransferResponse>>();
        Assert.NotNull(transfers);
        Assert.True(transfers.Count >= 1);
    }
}
