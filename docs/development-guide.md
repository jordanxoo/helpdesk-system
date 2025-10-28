# Development Guide

## Struktura Implementacji

Projekt jest szkieletem gotowym do rozwoju. Wszystkie miejsca wymagające implementacji są oznaczone komentarzami `// TODO:`.

## Kolejność Implementacji (Rekomendowana)

### 1. Shared Library (Wspólne komponenty)

#### Modele (`src/Shared/Models/`)
- `User.cs` - Pełne właściwości użytkownika
- `Ticket.cs` - Pełne właściwości ticketu i komentarzy

#### DTOs (`src/Shared/DTOs/`)
- `AuthDTOs.cs` - Request/Response dla autentykacji
- `TicketDTOs.cs` - Request/Response dla ticketów

#### Events (`src/Shared/Events/`)
- `BaseEvent.cs` - Bazowa klasa eventów z Id i Timestamp
- `TicketEvents.cs` - Eventy związane z ticketami

#### Configuration (`src/Shared/Configuration/`)
- `JwtSettings.cs` - Konfiguracja JWT
- `DatabaseSettings.cs` - Ustawienia bazy danych
- `MessagingSettings.cs` - Ustawienia RabbitMQ/SQS

#### Messaging (`src/Shared/Messaging/`)
- `IMessagePublisher.cs` - Interfejs do publikowania wiadomości
- `IMessageConsumer.cs` - Interfejs do konsumowania wiadomości
- Implementacje dla RabbitMQ i AWS SQS

### 2. AuthService (Uwierzytelnianie)

#### Data Layer
- `ApplicationUser.cs` - Rozszerzenie IdentityUser
- `AuthDbContext.cs` - Konfiguracja DbContext z Identity

#### Services
- `ITokenService.cs` / `TokenService.cs` - Generowanie i walidacja JWT tokenów

#### Controllers
- `AuthController.cs` - Endpointy:
  - POST /api/auth/register
  - POST /api/auth/login
  - POST /api/auth/refresh-token
  - POST /api/auth/logout

#### Program.cs
- Konfiguracja Identity
- Konfiguracja JWT Authentication
- Konfiguracja DbContext

### 3. UserService (Zarządzanie użytkownikami)

#### Data Layer
- `UserDbContext.cs` - Konfiguracja encji User

#### Repositories
- `IUserRepository.cs` / `UserRepository.cs` - CRUD operacje

#### Services
- `IUserService.cs` / `UserServiceImpl.cs` - Logika biznesowa

#### Controllers
- `UsersController.cs` - Endpointy:
  - GET /api/users (lista użytkowników)
  - GET /api/users/{id}
  - PUT /api/users/{id}
  - DELETE /api/users/{id}

### 4. TicketService (Zarządzanie zgłoszeniami)

#### Data Layer
- `TicketDbContext.cs` - Konfiguracja encji Ticket i TicketComment

#### Repositories
- `ITicketRepository.cs` / `TicketRepository.cs` - CRUD operacje

#### Services
- `ITicketService.cs` / `TicketServiceImpl.cs` - Logika biznesowa
- Publikowanie eventów (ticket-created, ticket-assigned, etc.)

#### Controllers
- `TicketsController.cs` - Endpointy:
  - GET /api/tickets (z filtrowaniem i paginacją)
  - GET /api/tickets/{id}
  - POST /api/tickets
  - PUT /api/tickets/{id}
  - DELETE /api/tickets/{id}
  - POST /api/tickets/{id}/comments

### 5. NotificationService (Powiadomienia)

#### Services
- `IEmailService.cs` / `EmailService.cs` - Wysyłanie emaili (SMTP/AWS SES)
- `ISmsService.cs` / `SmsService.cs` - Wysyłanie SMS (AWS SNS)

#### Workers
- `NotificationWorker.cs` - Background service konsumujący eventy z kolejki

### 6. ApiGateway

#### ocelot.json
- Konfiguracja routingu dla wszystkich serwisów
- Konfiguracja authentication
- Rate limiting, load balancing

### 7. Database Migrations

**Auto-Migration (Development):**
Wszystkie serwisy automatycznie wykonują migracje przy starcie w Development mode (`Program.cs`):

```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
    dbContext.Database.Migrate();  // Automatyczne migracje
}
```

**Flow przy pierwszym uruchomieniu:**
1. PostgreSQL kontener tworzy 3 puste bazy (`init-db.sql`)
2. Każdy serwis wykonuje swoje migracje przy starcie
3. Tabele są automatycznie tworzone

**Tworzenie nowych migracji (gdy zmieniasz modele):**
```bash
# AuthService
cd src/AuthService
dotnet ef migrations add MigrationName
# Restart kontenera - migracja wykona się automatycznie

# TicketService
cd src/TicketService
dotnet ef migrations add MigrationName

# UserService
cd src/UserService
dotnet ef migrations add MigrationName
```

**Reset bazy od zera:**
```bash
docker-compose down -v  # Usuwa volumeny
docker-compose up --build  # init-db.sql + auto-migrations uruchomią się ponownie
```

### 8. Docker Configuration

- Sprawdzenie i aktualizacja `docker-compose.yml`
- Testowanie uruchomienia wszystkich serwisów

### 9. AWS Infrastructure

- Uzupełnienie CloudFormation templates
- Konfiguracja ECS Task Definitions
- Konfiguracja RDS, SQS, Load Balancer

### 10. Testing

- Unit tests dla każdego serwisu
- Integration tests
- End-to-end tests

## Workflow Developmentu

1. **Setup środowiska** (pierwszy raz):
   ```bash
   cp .env.example .env  # Skopiuj konfigurację
   ```

2. **Wybierz komponent** do implementacji (np. AuthService)
3. **Przeczytaj komentarze TODO** w plikach
4. **Implementuj funkcjonalność** krok po kroku
5. **Testuj lokalnie** bez Dockera
6. **Testuj z Docker Compose**
7. **Commit zmian** do git

## Przykładowy Flow dla AuthService

```bash
# 1. Implementuj modele w Shared
# Edytuj: src/Shared/Models/User.cs
# Edytuj: src/Shared/DTOs/AuthDTOs.cs

# 2. Implementuj TokenService
# Edytuj: src/AuthService/Services/TokenService.cs

# 3. Implementuj AuthController
# Edytuj: src/AuthService/Controllers/AuthController.cs

# 4. Konfiguruj Program.cs
# Edytuj: src/AuthService/Program.cs

# 5. Dodaj migrację
cd src/AuthService
dotnet ef migrations add InitialCreate

# 6. Uruchom i testuj
dotnet run

# 7. Test z Postman/curl
curl -X POST http://localhost:5101/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test123!"}'
```

## Pomocne Komendy

### .NET CLI
```bash
# Build całego solution
dotnet build

# Restore packages
dotnet restore

# Uruchom serwis
dotnet run --project src/AuthService

# Dodaj migrację
dotnet ef migrations add MigrationName --project src/ServiceName

# Aktualizuj bazę
dotnet ef database update --project src/ServiceName

# Dodaj nowy projekt
dotnet new webapi -n NewService -o src/NewService
```

### Docker
```bash
# Build wszystkich serwisów
docker-compose build

# Uruchom wszystkie serwisy
docker-compose up -d

# Zobacz logi
docker-compose logs -f service-name

# Restart serwisu
docker-compose restart service-name

# Zatrzymaj wszystko
docker-compose down

# Usuń wszystko łącznie z volumami
docker-compose down -v
```

### Git
```bash
# Dodaj zmiany
git add .

# Commit
git commit -m "Implemented AuthService login endpoint"

# Push
git push origin main
```

## Tips & Best Practices

1. **Zawsze implementuj interfejs przed implementacją** (SOLID principles)
2. **Używaj Dependency Injection** dla wszystkich serwisów
3. **Waliduj dane wejściowe** w kontrolerach
4. **Loguj wszystkie ważne operacje** (użyj ILogger)
5. **Obsługuj błędy** globalnie (middleware do exception handling)
6. **Testuj każdy endpoint** przed przejściem dalej
7. **Dokumentuj API** w Swagger
8. **Używaj async/await** dla wszystkich operacji I/O
9. **Stosuj Code First** dla Entity Framework
10. **Commituj często** małe, atomowe zmiany

## Pytania do rozważenia podczas implementacji

- Jak obsłużyć błędy i wyjątki?
- Jak zabezpieczyć endpointy?
- Jak paginować duże kolekcje?
- Jak logować operacje?
- Jak testować funkcjonalność?
- Jak migrować dane w przyszłości?
- Jak monitorować wydajność?

## Health Checks

Każdy serwis ma `/health` endpoint który sprawdza połączenie z PostgreSQL.

**Przykład response:**
```json
{
  "status": "Healthy",
  "service": "AuthService",
  "timestamp": "2025-10-28T12:38:54Z",
  "checks": [
    {
      "name": "npgsql",
      "status": "Healthy",
      "duration": 2.75
    }
  ]
}
```

**Pakiet:**
```xml
<PackageReference Include="AspNetCore.HealthChecks.Npgsql" Version="9.0.0" />
```

**Dlaczego nie ma w Swagger?**
- `/health` to infrastructure endpoint dla Docker/Kubernetes/ECS
- Swagger to business API dla deweloperów
- To różne rzeczy, celowo oddzielone

## Następne Kroki

Po zakończeniu implementacji szkieletu:
1. Frontend (React/Angular/Blazor)
2. Real-time notifications (SignalR)
3. File attachments dla ticketów
4. Advanced search i filtering
5. Reporting i analytics
6. Monitoring (Prometheus, Grafana)
7. CI/CD Pipeline (GitHub Actions)
8. Load testing
9. Security audit
10. Production deployment na AWS

## 🔒 Ważne Informacje o Bezpieczeństwie

### HTTPS i SSL/TLS

**Kontenery NIE używają `UseHttpsRedirection()`** - to zamierzone!

#### Dlaczego?

1. **Development/Docker:**
   - Kontenery komunikują się przez HTTP w sieci Docker
   - Brak potrzeby szyfrowania localhost
   - Prostsze debugowanie

2. **Production/AWS:**
   - AWS ALB (Application Load Balancer) obsługuje SSL/TLS termination
   - Certyfikat SSL zarządzany przez AWS Certificate Manager
   - ALB przekazuje ruch do kontenerów przez HTTP w bezpiecznej sieci VPC

#### Architektura bezpieczeństwa w AWS:

```
Internet → HTTPS :443 
          ↓
       AWS ALB (SSL Cert)
          ↓ SSL Termination
          ↓
       HTTP :8080 (VPC - prywatna)
          ↓
       ECS Containers
```

#### Ustawienie JWT:

```csharp
options.RequireHttpsMetadata = false; // ✅ Prawidłowe dla SSL termination
```

To pozwala na walidację tokenów JWT przez HTTP, bo HTTPS jest już obsłużony przez ALB.

**❌ NIE dodawaj:**
- `app.UseHttpsRedirection()` - spowoduje redirect loops
- `options.RequireHttpsMetadata = true` - będzie blokować w Docker/AWS

Więcej: `docs/architecture.md` → sekcja "Bezpieczeństwo i HTTPS"

```
