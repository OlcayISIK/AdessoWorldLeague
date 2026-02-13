# Adesso World League

A .NET 10 Web API that simulates a football league group draw system. Randomly assigns 32 teams from 8 countries into 4 or 8 groups, ensuring no two teams from the same country end up in the same group.

## Architecture

7-layer monolithic architecture:

| Layer | Description |
|-------|-------------|
| **Core** | Base entities, enums, constants, result pattern |
| **Mongo** | MongoDB context, settings, DI extensions |
| **Data** | MongoDB document models |
| **Dto** | Request/response DTOs with validation |
| **Repository** | Generic repository pattern with soft delete |
| **Business** | Services, JWT auth, draw algorithm, localization |
| **WebApi** | Controllers, middleware, program configuration |

## Tech Stack

- .NET 10
- MongoDB (via MongoDB.Driver)
- JWT Authentication (BCrypt password hashing)
- Serilog (Console + Seq sinks)
- Docker & Docker Compose
- Rate Limiting (fixed window)
- Localization (EN / TR)

## API Endpoints

### Auth (`/api/auth`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register a new user |
| POST | `/api/auth/login` | No | Login and get JWT token |

### Draw (`/api/draw`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/draw/performDraw` | Yes | Perform a new group draw |
| GET | `/api/draw/{id}` | Yes | Get draw by ID |
| GET | `/api/draw/getAllDraws` | Yes | Get all draws |

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker & Docker Compose (for containerized setup)
- MongoDB (for local development without Docker)

### Run with Docker

```bash
docker-compose up --build
```

This starts:
- **API** on `http://localhost:5000`
- **MongoDB** on `localhost:27017`
- **Seq** (log viewer) on `http://localhost:8081`

### Run Locally

1. Start MongoDB on `localhost:27017`
2. (Optional) Start Seq on `localhost:5341`
3. Run the API:

```bash
dotnet run --project AdessoWorldLeague.WebApi
```

## Usage

### 1. Register

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "user@test.com", "password": "Pass123!", "firstName": "John", "lastName": "Doe"}'
```

### 2. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "user@test.com", "password": "Pass123!"}'
```

### 3. Perform Draw

```bash
curl -X POST http://localhost:5000/api/draw/performDraw \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"groupCount": 4}'
```

## Localization

The API supports English (default) and Turkish. Set the `Accept-Language` header:

```
Accept-Language: tr
```

## Configuration

Configuration is in `AdessoWorldLeague.WebApi/appsettings.json`. Docker Compose overrides MongoDB and Seq connection strings for container networking.

| Setting | Description |
|---------|-------------|
| `MongoSettings:ConnectionString` | MongoDB connection string |
| `MongoSettings:DatabaseName` | Database name |
| `JwtSettings:Secret` | JWT signing key |
| `JwtSettings:ExpirationMinutes` | Token expiration time |
| `Seq:ServerUrl` | Seq log server URL |

## Project Structure

```
AdessoWorldLeague/
├── AdessoWorldLeague.Core/           # Base entities, enums, constants
├── AdessoWorldLeague.Mongo/          # MongoDB infrastructure
├── AdessoWorldLeague.Data/           # Document models
├── AdessoWorldLeague.Dto/            # DTOs
├── AdessoWorldLeague.Repository/     # Data access layer
├── AdessoWorldLeague.Business/       # Business logic & services
├── AdessoWorldLeague.WebApi/         # API entry point
├── Dockerfile
├── docker-compose.yml
└── AdessoWorldLeague.slnx
```
