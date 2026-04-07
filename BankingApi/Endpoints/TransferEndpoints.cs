using BankingApi.DTOs;
using BankingApi.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Endpoints;

public static class TransferEndpoints
{
    public static IEndpointRouteBuilder MapTransferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transfers")
            .WithTags("Transfers");

        group.MapGet("/", GetAllTransfers)
            .WithName("GetAllTransfers")
            .WithSummary("List all transfers");

        group.MapGet("/{id:guid}", GetTransferById)
            .WithName("GetTransferById")
            .WithSummary("Get a transfer by ID");

        group.MapPost("/", CreateTransfer)
            .WithName("CreateTransfer")
            .WithSummary("Initiate a new bank transfer");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<TransferResponse>>> GetAllTransfers(
        ITransferService service,
        CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(ct));

    private static async Task<Results<Ok<TransferResponse>, NotFound<ProblemDetails>>> GetTransferById(
        Guid id,
        ITransferService service,
        CancellationToken ct)
    {
        var transfer = await service.GetByIdAsync(id, ct);
        return TypedResults.Ok(transfer);
    }

    private static async Task<Results<Created<TransferResponse>, ValidationProblem, NotFound<ProblemDetails>, UnprocessableEntity<ProblemDetails>>> CreateTransfer(
        CreateTransferRequest request,
        ITransferService service,
        IValidator<CreateTransferRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var transfer = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/transfers/{transfer.Id}", transfer);
    }
}
