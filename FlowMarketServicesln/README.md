# FlowMarketServicesln

Production-oriented full-stack foundation:

- Backend: ASP.NET Core 10 Web API, EF Core 10, PostgreSQL, Identity, JWT + rotating refresh tokens, MediatR CQRS, FluentValidation, AutoMapper, Serilog, Swagger + Scalar, rate limiting.
- Frontend: Next.js 14 App Router, strict TypeScript, Tailwind CSS, React Query v5, RHF + Zod, Axios interceptors.

## Prerequisites

- .NET SDK 10.x
- Node.js 20+ with npm available in PATH
- PostgreSQL 16+

## Backend run

1. Configure development secrets in `src/FlowMarket.Api/appsettings.Development.json`.
2. From `FlowMarketServicesln`:
   - `dotnet restore`
   - `dotnet ef migrations add Initial --project src/FlowMarket.Infrastructure --startup-project src/FlowMarket.Api`
   - `dotnet ef database update --project src/FlowMarket.Infrastructure --startup-project src/FlowMarket.Api`
   - `dotnet run --project src/FlowMarket.Api`

## Frontend run

1. Copy `frontend/flowmarket-web/.env.example` to `.env.local`.
2. From `frontend/flowmarket-web`:
   - `npm install`
   - `npm run dev`

## API docs

- Swagger: `/swagger`
- Scalar: `/scalar/v1`
