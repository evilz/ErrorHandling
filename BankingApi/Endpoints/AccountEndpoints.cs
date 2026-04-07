using BankingApi.DTOs;
using BankingApi.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts")
            .WithTags("Accounts");

        group.MapGet("/", GetAllAccounts)
            .WithName("GetAllAccounts")
            .WithSummary("List all bank accounts");

        group.MapGet("/{id:guid}", GetAccountById)
            .WithName("GetAccountById")
            .WithSummary("Get a bank account by ID");

        group.MapGet("/{id:guid}/balance", GetAccountBalance)
            .WithName("GetAccountBalance")
            .WithSummary("Get the balance of a bank account");

        group.MapGet("/{id:guid}/transfers", GetAccountTransfers)
            .WithName("GetAccountTransfers")
            .WithSummary("List transfers for an account");

        group.MapGet("/{id:guid}/cards", GetAccountCards)
            .WithName("GetAccountCards")
            .WithSummary("List cards linked to an account");

        group.MapPost("/", CreateAccount)
            .WithName("CreateAccount")
            .WithSummary("Create a new bank account");

        group.MapPut("/{id:guid}", UpdateAccount)
            .WithName("UpdateAccount")
            .WithSummary("Update a bank account");

        group.MapDelete("/{id:guid}", CloseAccount)
            .WithName("CloseAccount")
            .WithSummary("Close a bank account");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<AccountResponse>>> GetAllAccounts(
        IAccountService service,
        CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(ct));

    private static async Task<Results<Ok<AccountResponse>, NotFound<ProblemDetails>>> GetAccountById(
        Guid id,
        IAccountService service,
        CancellationToken ct)
    {
        var account = await service.GetByIdAsync(id, ct);
        return TypedResults.Ok(account);
    }

    private static async Task<Results<Ok<AccountBalanceResponse>, NotFound<ProblemDetails>>> GetAccountBalance(
        Guid id,
        IAccountService service,
        CancellationToken ct)
    {
        var balance = await service.GetBalanceAsync(id, ct);
        return TypedResults.Ok(balance);
    }

    private static async Task<Ok<IReadOnlyList<TransferResponse>>> GetAccountTransfers(
        Guid id,
        ITransferService transferService,
        CancellationToken ct) =>
        TypedResults.Ok(await transferService.GetByAccountIdAsync(id, ct));

    private static async Task<Ok<IReadOnlyList<CardResponse>>> GetAccountCards(
        Guid id,
        ICardService cardService,
        CancellationToken ct) =>
        TypedResults.Ok(await cardService.GetByAccountIdAsync(id, ct));

    private static async Task<Results<Created<AccountResponse>, ValidationProblem>> CreateAccount(
        CreateAccountRequest request,
        IAccountService service,
        IValidator<CreateAccountRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var account = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/accounts/{account.Id}", account);
    }

    private static async Task<Results<Ok<AccountResponse>, NotFound<ProblemDetails>, ValidationProblem, UnprocessableEntity<ProblemDetails>>> UpdateAccount(
        Guid id,
        UpdateAccountRequest request,
        IAccountService service,
        IValidator<UpdateAccountRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var account = await service.UpdateAsync(id, request, ct);
        return TypedResults.Ok(account);
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>, Conflict<ProblemDetails>, UnprocessableEntity<ProblemDetails>>> CloseAccount(
        Guid id,
        IAccountService service,
        CancellationToken ct)
    {
        await service.CloseAsync(id, ct);
        return TypedResults.NoContent();
    }
}
