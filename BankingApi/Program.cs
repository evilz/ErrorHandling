using BankingApi.Endpoints;
using BankingApi.Infrastructure;
using BankingApi.Repositories;
using BankingApi.Services;
using BankingApi.Validation;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ─── OpenAPI ───────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title = "Banking API";
        doc.Info.Version = "v1";
        doc.Info.Description = "A minimal REST API for managing bank accounts, cards, and transfers built with .NET 10.";
        return Task.CompletedTask;
    });
});

// ─── Problem Details ───────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ─── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ─── Validation ────────────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<CreateAccountRequestValidator>();

// ─── Repositories (Singleton for in-memory state) ─────────────────────────────
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<ICardRepository, InMemoryCardRepository>();
builder.Services.AddSingleton<ITransferRepository, InMemoryTransferRepository>();

// ─── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ITransferService, TransferService>();

// ─── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ─── Endpoints ─────────────────────────────────────────────────────────────────
app.MapAccountEndpoints();
app.MapCardEndpoints();
app.MapTransferEndpoints();

app.MapHealthChecks("/health").WithTags("Health");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
