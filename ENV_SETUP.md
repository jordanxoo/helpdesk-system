# Environment Setup Guide

## Quick Start

```bash
cp .env.example .env
docker-compose up --build
```

## Why .env?

`.env` contains secrets (gitignored) → `.env.example` is the template (committed)

## Key Variables

**JWT (MUST be identical across all services):**
```bash
JWT_SECRET=your-secret-key-min-32-characters-long
JWT_ISSUER=HelpdeskSystem
JWT_AUDIENCE=HelpdeskClients
```

**Database & RabbitMQ:**
```bash
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```

## How It Works

1. Docker Compose reads `.env` automatically
2. Env vars injected as `JwtSettings__SecretKey`, `ConnectionStrings__DefaultConnection`
3. ASP.NET Core: env vars **override** `appsettings.json` values

## Common Issues

**Token validation fails?** → Check `JWT_SECRET` is identical in `.env`  
**Missing .env?** → `cp .env.example .env`
