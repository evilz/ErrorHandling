# Banking API — .NET 10 Minimal API

A production-ready **Minimal REST API** built with **.NET 10** themed around a banking system: bank accounts, card management, and fund transfers.

## Features

- **Accounts** – Create, read, update, and close bank accounts; query balance; list linked transfers and cards
- **Cards** – Issue debit/credit cards, activate, block, and cancel them
- **Transfers** – Initiate transfers between accounts with real-time balance updates

## Architecture & Best Practices

| Area | Approach |
|------|----------|
| Framework | .NET 10 Minimal API |
| Endpoint organization | `IEndpointRouteBuilder` extension methods per feature |
| Typed results | `TypedResults` / `Results<T1, T2, …>` for full OpenAPI inference |
| Error handling | `IExceptionHandler` + RFC 9457 Problem Details |
| Validation | FluentValidation with inline endpoint validation |
| OpenAPI | `AddOpenApi()` / `MapOpenApi()` (native .NET 10) |
| Health checks | `/health` endpoint |
| Repository pattern | In-memory, thread-safe (`ConcurrentDictionary`) |
| CORS | Configured for development |

## Project Structure

```
BankingApi/
├── Domain/
│   ├── Models/        # Account, Card, Transfer
│   └── Enums/         # AccountStatus, CardStatus, CardType, TransferStatus
├── DTOs/              # Request/Response records
├── Repositories/      # IAccountRepository, ICardRepository, ITransferRepository
├── Services/          # Business logic (AccountService, CardService, TransferService)
├── Endpoints/         # AccountEndpoints, CardEndpoints, TransferEndpoints
├── Validation/        # FluentValidation validators
└── Infrastructure/    # In-memory repositories, exception handler, domain exceptions

BankingApi.Tests/
├── AccountsIntegrationTests.cs
├── CardsIntegrationTests.cs
└── TransfersIntegrationTests.cs
```

## API Endpoints

### Accounts
| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/accounts` | List all accounts |
| POST | `/api/accounts` | Create a new account |
| GET | `/api/accounts/{id}` | Get account by ID |
| PUT | `/api/accounts/{id}` | Update account |
| DELETE | `/api/accounts/{id}` | Close account |
| GET | `/api/accounts/{id}/balance` | Get account balance |
| GET | `/api/accounts/{id}/transfers` | List transfers for account |
| GET | `/api/accounts/{id}/cards` | List cards for account |

### Cards
| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/cards` | List all cards |
| POST | `/api/cards` | Issue a new card |
| GET | `/api/cards/{id}` | Get card by ID |
| PUT | `/api/cards/{id}/activate` | Activate a card |
| PUT | `/api/cards/{id}/block` | Block a card |
| DELETE | `/api/cards/{id}` | Cancel a card |

### Transfers
| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/transfers` | List all transfers |
| POST | `/api/transfers` | Initiate a transfer |
| GET | `/api/transfers/{id}` | Get transfer by ID |

### Health
| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Health check |

## Getting Started

```bash
cd BankingApi
dotnet run
```

OpenAPI document available at: `http://localhost:5000/openapi/v1.json`

## Running Tests

```bash
dotnet test
```

All 24 integration tests cover accounts, cards, and transfers including happy paths and business rule violations.
