using System.Net;
using System.Net.Http.Json;
using BankingApi.Domain.Enums;
using BankingApi.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankingApi.Tests;

public class AccountsIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAllAccounts_ReturnsOkWithEmptyList()
    {
        var response = await _client.GetAsync("/api/accounts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<AccountResponse>>();
        Assert.NotNull(accounts);
    }

    [Fact]
    public async Task CreateAccount_WithValidData_ReturnsCreated()
    {
        var request = new CreateAccountRequest("Alice Dupont", "owner-001", "EUR", 1000m);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(account);
        Assert.Equal("Alice Dupont", account.OwnerName);
        Assert.Equal(1000m, account.Balance);
        Assert.Equal("EUR", account.Currency);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.NotEqual(Guid.Empty, account.Id);
    }

    [Fact]
    public async Task CreateAccount_WithInvalidData_ReturnsBadRequest()
    {
        var request = new CreateAccountRequest("", "owner-002", "INVALID", -100m);

        var response = await _client.PostAsJsonAsync("/api/accounts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_WhenExists_ReturnsOk()
    {
        var created = await CreateTestAccountAsync("Bob Martin", "owner-003");

        var response = await _client.GetAsync($"/api/accounts/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(account);
        Assert.Equal(created.Id, account.Id);
    }

    [Fact]
    public async Task GetAccountById_WhenNotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountBalance_ReturnsCorrectBalance()
    {
        var created = await CreateTestAccountAsync("Carol Stone", "owner-004", 500m);

        var response = await _client.GetAsync($"/api/accounts/{created.Id}/balance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var balance = await response.Content.ReadFromJsonAsync<AccountBalanceResponse>();
        Assert.NotNull(balance);
        Assert.Equal(500m, balance.Balance);
    }

    [Fact]
    public async Task UpdateAccount_WithValidData_ReturnsOk()
    {
        var created = await CreateTestAccountAsync("Dave Becker", "owner-005");
        var updateRequest = new UpdateAccountRequest("Dave Updated");

        var response = await _client.PutAsJsonAsync($"/api/accounts/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AccountResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Dave Updated", updated.OwnerName);
    }

    [Fact]
    public async Task CloseAccount_WithZeroBalance_ReturnsNoContent()
    {
        var created = await CreateTestAccountAsync("Eve Zero", "owner-006", 0m);

        var response = await _client.DeleteAsync($"/api/accounts/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CloseAccount_WithNonZeroBalance_Returns422()
    {
        var created = await CreateTestAccountAsync("Frank Rich", "owner-007", 500m);

        var response = await _client.DeleteAsync($"/api/accounts/{created.Id}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    internal async Task<AccountResponse> CreateTestAccountAsync(
        string ownerName, string ownerId, decimal balance = 0m)
    {
        var request = new CreateAccountRequest(ownerName, ownerId, "EUR", balance);
        var response = await _client.PostAsJsonAsync("/api/accounts", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>())!;
    }
}
