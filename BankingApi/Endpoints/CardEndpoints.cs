using BankingApi.DTOs;
using BankingApi.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Endpoints;

public static class CardEndpoints
{
    public static IEndpointRouteBuilder MapCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cards")
            .WithTags("Cards");

        group.MapGet("/", GetAllCards)
            .WithName("GetAllCards")
            .WithSummary("List all cards");

        group.MapGet("/{id:guid}", GetCardById)
            .WithName("GetCardById")
            .WithSummary("Get a card by ID");

        group.MapPost("/", CreateCard)
            .WithName("CreateCard")
            .WithSummary("Issue a new card for an account");

        group.MapPut("/{id:guid}/activate", ActivateCard)
            .WithName("ActivateCard")
            .WithSummary("Activate a card");

        group.MapPut("/{id:guid}/block", BlockCard)
            .WithName("BlockCard")
            .WithSummary("Block a card");

        group.MapDelete("/{id:guid}", CancelCard)
            .WithName("CancelCard")
            .WithSummary("Cancel a card");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<CardResponse>>> GetAllCards(
        ICardService service,
        CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(ct));

    private static async Task<Results<Ok<CardResponse>, NotFound<ProblemDetails>>> GetCardById(
        Guid id,
        ICardService service,
        CancellationToken ct)
    {
        var card = await service.GetByIdAsync(id, ct);
        return TypedResults.Ok(card);
    }

    private static async Task<Results<Created<CardResponse>, ValidationProblem, NotFound<ProblemDetails>, UnprocessableEntity<ProblemDetails>>> CreateCard(
        CreateCardRequest request,
        ICardService service,
        IValidator<CreateCardRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var card = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/cards/{card.Id}", card);
    }

    private static async Task<Results<Ok<CardResponse>, NotFound<ProblemDetails>, Conflict<ProblemDetails>, UnprocessableEntity<ProblemDetails>>> ActivateCard(
        Guid id,
        ICardService service,
        CancellationToken ct)
    {
        var card = await service.ActivateAsync(id, ct);
        return TypedResults.Ok(card);
    }

    private static async Task<Results<Ok<CardResponse>, NotFound<ProblemDetails>, Conflict<ProblemDetails>, UnprocessableEntity<ProblemDetails>>> BlockCard(
        Guid id,
        ICardService service,
        CancellationToken ct)
    {
        var card = await service.BlockAsync(id, ct);
        return TypedResults.Ok(card);
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>, Conflict<ProblemDetails>>> CancelCard(
        Guid id,
        ICardService service,
        CancellationToken ct)
    {
        await service.CancelAsync(id, ct);
        return TypedResults.NoContent();
    }
}
