# Helpdesk System - System Rozproszonej Pomocy Technicznej

System Helpdesk zbudowany jako rozproszona aplikacja mikroservisowa, gotowa do deploymentu na AWS.

## 🏗️ Architektura

System składa się z następujących mikroservisów:

### Mikroservisy
- **AuthService** (Port 5101) - Kontekst autentykacji: dane logowania (email, hasło), tokeny JWT
- **UserService** (Port 5103) - Kontekst użytkownika: dane profilu (imię, nazwisko, telefon, rola) + dane biznesowe (organizacja, aktywność)
- **TicketService** (Port 5102) - Zarządzanie zgłoszeniami helpdesk
- **NotificationService** (Port 5104) - Wysyłanie powiadomień (email, SMS)
- **ApiGateway** (Port 5100) - Ocelot API Gateway dla routingu żądań

### Infrastruktura
- **PostgreSQL** - Bazy danych dla każdego serwisu
- **RabbitMQ** - Message broker dla komunikacji asynchronicznej między serwisami
- **Docker & Docker Compose** - Konteneryzacja i orkiestracja lokalna
- **AWS ECS/Fargate** - Deployment w chmurze (CloudFormation templates)

### Decyzje Architektoniczne

System wykorzystuje wzorzec **Database per Service** - każdy mikroservis ma własną bazę danych (helpdesk_auth, helpdesk_users, helpdesk_tickets). To podejście zapewnia:
- **Separację odpowiedzialności** - każdy serwis jest niezależny
- **Skalowalność** - serwisy można skalować osobno
- **Odporność na awarie** - problemy w jednym serwisie nie blokują innych

**Komunikacja między serwisami:**
- **Synchroniczna (HTTP)**: TicketService → UserService (pobieranie organizacji użytkownika przy tworzeniu ticketu)
- **Asynchroniczna (RabbitMQ)**: AuthService → UserService (synchronizacja nowych użytkowników)

**Trade-off**: Brak foreign key constraints między bazami (np. organization_id w users → organizations w innej bazie). To normalne w architekturze mikroservisowej - walidacja odbywa się w kodzie aplikacji, co zapewnia eventual consistency.

## 📁 Struktura Projektu

```
helpdesk-system/
├── src/
│   ├── Shared/                  # Wspólne modele, DTOs, events, messaging
│   ├── AuthService/             # Serwis uwierzytelniania
│   ├── TicketService/           # Serwis zgłoszeń
│   ├── UserService/             # Serwis użytkowników
│   ├── NotificationService/     # Serwis powiadomień
│   └── ApiGateway/              # API Gateway (Ocelot)
├── infrastructure/              # AWS CloudFormation/CDK templates
├── docker/                      # Skrypty Docker
├── docs/                        # Dokumentacja
└── docker-compose.yml           # Konfiguracja Docker Compose
```

## 🚀 Quick Start

### Wymagania
- .NET 9.0 SDK
- Docker i Docker Compose
- PostgreSQL (lub użyj Docker Compose)
- RabbitMQ (lub użyj Docker Compose)

### Uruchomienie Lokalne z Docker Compose

```bash
# 1. Skopiuj plik konfiguracyjny (pierwszy raz)
cp .env.example .env

# 2. Zbudowanie i uruchomienie wszystkich serwisów
docker-compose up --build

# W tle
docker-compose up -d --build

# Sprawdzenie statusu
docker-compose ps

# Logi
docker-compose logs -f

# Zatrzymanie
docker-compose down
```

### Dostęp do Serwisów

**Przez API Gateway (ZALECANE dla frontendu):**
- **API Gateway**: http://localhost:5100
  - Auth: `http://localhost:5100/api/auth/*`
  - Tickets: `http://localhost:5100/api/tickets/*`
  - Users: `http://localhost:5100/api/users/*`

**Bezpośredni dostęp do serwisów (dla testowania/debugowania):**
- **Auth Service**: http://localhost:5101/api/auth/* (Swagger: /swagger, Health: /health)
- **Ticket Service**: http://localhost:5102/api/tickets/* (Swagger: /swagger, Health: /health)
- **User Service**: http://localhost:5103/api/users/* (Swagger: /swagger, Health: /health)
- **Notification Service**: http://localhost:5104 (Swagger: /swagger, Health: /health)
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

> **Uwaga:** Frontend powinien używać **wyłącznie API Gateway** (port 5100). Bezpośrednie porty serwisów są tylko do lokalnego testowania.

### Uruchomienie Lokalne bez Docker

```bash
# Restore dependencies
dotnet restore

# Uruchomienie poszczególnych serwisów
dotnet run --project src/AuthService
dotnet run --project src/TicketService
dotnet run --project src/UserService
dotnet run --project src/NotificationService
dotnet run --project src/ApiGateway
```

### Bazy Danych

Każdy mikroserws ma własną bazę danych (Database per Service pattern):
- `helpdesk_auth` - AuthService
- `helpdesk_tickets` - TicketService
- `helpdesk_users` - UserService

### Migracje Entity Framework

```bash
# Dodanie migracji
dotnet ef migrations add InitialCreate --project src/AuthService

# Aktualizacja bazy danych
dotnet ef database update --project src/AuthService
```

## 📋 Changelog - Październik 2025

### ✅ Zaimplementowano (28.10.2025)
1. **RabbitMQ Integration** - Pełna komunikacja event-driven między serwisami
   - Publisher w TicketService publikuje eventy
   - Consumer w NotificationService odbiera i przetwarza eventy
   - Exchange: `helpdesk-events` (Topic), persistent messages, auto-recovery

2. **NotificationService - Kompletna implementacja**
   - EmailService z SMTP (MailKit)
   - SmsService (szkielet, gotowy do integracji z AWS SNS/Twilio)
   - NotificationWorker (BackgroundService) konsumujący 4 typy eventów
   - Automatyczne wysyłanie powiadomień o ticketach

3. **UserService - Rozszerzenie funkcjonalności**
   - JWT Authentication z role-based authorization
   - Kompletny UsersController z 10 endpointami
   - GET /api/users - lista z paginacją (Agent/Admin)
   - POST /api/users/search - wyszukiwanie z filtrami
   - GET /api/users/me - profil zalogowanego użytkownika
   - POST/PUT/DELETE - zarządzanie użytkownikami (Admin)

4. **Konfiguracja portów (launchSettings.json)**
   - AuthService: 5101
   - TicketService: 5102
   - UserService: 5103
   - NotificationService: 5104

5. **Health Checks - JSON response**
   - Wszystkie serwisy zwracają ujednolicony format JSON
   - Status, service name, timestamp, checks (PostgreSQL/RabbitMQ)

### 📁 Dodane pliki
- `insomnia-collection.json` - Gotowa kolekcja API do testowania
- `src/NotificationService/Services/*` - Email/SMS services
- `src/NotificationService/Workers/NotificationWorker.cs`
- `src/Shared/Messaging/RabbitMqPublisher.cs`
- `src/Shared/Messaging/RabbitMqConsumer.cs`
- `src/*/Properties/launchSettings.json` - Wszystkie serwisy

### 🔧 Zaktualizowane
- Shared Events - Rozszerzone o dodatkowe pola (email, phone, content)
- TicketService - Publikowanie eventów po każdej akcji
- appsettings.json - Dodano MessagingSettings dla RabbitMQ
- `helpdesk_users` - UserService

---

## 📅 Changelog - Listopad 2025

### ✨ Faza 2: Rozszerzenie bazy danych + Komunikacja między serwisami

#### 🗄️ Rozszerzenie struktury bazy danych
- **TicketService** - Dodano tabele: `organizations`, `slas`, `tags`, `ticket_tags`, `ticket_history`, `attachments`
- **UserService** - Dodano pole `organization_id` (UUID) do tabeli `users`
- Nowe kontrolery: OrganizationsController, SlaController, TagsController
- Migracje dla wszystkich zmian struktury baz danych

#### 🔄 Komunikacja synchroniczna (HTTP)
- **UserServiceClient** - HTTP client dla komunikacji TicketService → UserService
- Auto-fetch organizacji użytkownika przy tworzeniu ticketu
- Opcja manualnego override `organizationId` przez Agent/Admin
- Timeout 10s, obsługa błędów, logging

#### 📨 Komunikacja asynchroniczna (Event-Driven)
- **UserRegisteredEvent** - Event publikowany przez AuthService po rejestracji
- **UserEventConsumer** - Worker w UserService nasłuchujący na eventy rejestracji
- Synchronizacja użytkowników: AuthService (helpdesk_auth) → UserService (helpdesk_users)
- Idempotencja - duplikaty eventów są ignorowane

#### 🎫 Tworzenie ticketów - rozszerzenie logiki biznesowej
- **Customer** - tworzy tickety dla siebie (userId z tokenu JWT)
- **Agent/Administrator** - tworzą tickety w imieniu klienta (wymagane `customerId` w request)
- Walidacja bezpieczeństwa - Customer nie może podać innego `customerId`
- Automatyczne przypisanie `organizationId` na podstawie użytkownika

#### 🔐 Role i autoryzacja
- POST /api/auth/register - dodano pole `role` (Customer, Agent, Administrator)
- PUT /api/users/{id}/organization - przypisanie użytkownika do organizacji (tylko Admin)
- POST /api/tickets - dostępne dla wszystkich ról z różną logiką

#### 📝 Dokumentacja
- Zaktualizowana kolekcja Insomnia - nowe requesty dla Agent workflow
- XML comments w kontrolerach opisujące zmiany
- Rozszerzona dokumentacja architektury w README

---

### 🔀 Clean Separation: AuthService vs UserService

#### Architektura
- **AuthService** - TYLKO credentials (email, hasło) + tokeny JWT
- **UserService** - WŁAŚCICIEL danych profilu (imię, nazwisko, telefon, rola) + dane biznesowe

#### Dlaczego duplikacja niektórych danych?
| Serwis | Rola | Email |
|--------|------|-------|
| AuthService | Uprawnienie (authorization) | Login (credential) |
| UserService | Funkcja biznesowa | Dana kontaktowa |

To świadoma decyzja - te same dane mają różny kontekst w różnych serwisach.

#### Fail-safe Registration
Rejestracja jest **atomowa** - jeśli publikacja eventu do RabbitMQ się nie powiedzie:
1. User jest usuwany z AuthService (rollback)
2. Frontend dostaje błąd 500
3. Brak desync między serwisami

---

## 📊 Message Queue

System używa RabbitMQ dla komunikacji event-driven między serwisami:

### Queues:
- `ticket-created` - Nowy ticket utworzony → wysyłka email/SMS do klienta
- `ticket-assigned` - Ticket przypisany do agenta → email do agenta
- `ticket-status-changed` - Status ticketu zmieniony → email do klienta
- `comment-added` - Komentarz dodany → email z powiadomieniem

**RabbitMQ Management**: http://localhost:15672 (guest/guest)

## ☁️ AWS Deployment

### Przygotowanie

1. Skonfiguruj AWS CLI
2. Zbuduj Docker images
3. Wypchnij images do Amazon ECR
4. Deploy CloudFormation stack

```bash
# Deploy infrastructure
aws cloudformation create-stack \
  --stack-name helpdesk-system \
  --template-body file://infrastructure/cloudformation-template.yaml \
  --parameters ParameterKey=Environment,ParameterValue=dev \
  --capabilities CAPABILITY_IAM
```

### Zasoby AWS (TODO: Implementacja)
- **ECS/Fargate** - Container orchestration
- **Application Load Balancer** - Load balancing
- **RDS PostgreSQL** - Managed databases
- **SQS** - Message queuing (alternatywa dla RabbitMQ)
- **SNS** - Push notifications
- **CloudWatch** - Logging i monitoring
- **S3** - File storage

## 🏥 Health Checks

Każdy serwis ma `/health` endpoint:

```bash
curl http://localhost:5101/health  # AuthService
curl http://localhost:5102/health  # TicketService
curl http://localhost:5103/health  # UserService
```

Response sprawdza PostgreSQL i zwraca status + czas wykonania w ms.

> **Uwaga:** To endpoint dla Docker/Kubernetes/ECS, nie dla frontendu. Dlatego nie ma go w Swagger.

## 🧪 Testing

```bash
# Uruchomienie testów
dotnet test

```

## 📝 API Documentation

Swagger UI dostępny dla każdego serwisu:
- Auth: http://localhost:5101/swagger
- Tickets: http://localhost:5102/swagger
- Users: http://localhost:5103/swagger
- Notifications: http://localhost:5104/swagger

## 🔐 Security

- **JWT Bearer Authentication** - Token-based auth z refresh tokens
- **Role-based Authorization** - Customer, Agent, Administrator
- **Unified JWT Config** - Wszystkie serwisy używają tego samego `JWT_SECRET` z `.env`
- **SSL/TLS Termination** - AWS ALB w production, HTTP w kontenerach
- **Database per Service** - Izolacja danych między serwisami
- **Secrets Management** - AWS Secrets Manager (production)
- **Password Requirements** - Minimum 6 znaków, mała litera wymagana

### Password Requirements API

Frontend może pobrać aktualne wymogi haseł dynamicznie:
```bash
GET http://localhost:5101/api/auth/password-requirements
```

Response:
```json
{
  "minimumLength": 6,
  "requireDigit": false,
  "requireLowercase": true,
  "requireUppercase": false,
  "requireNonAlphanumeric": false,
  "description": "Hasło musi mieć minimum 6 znaków, małą literę."
}
```

### Testowanie API

**Swagger UI (dokumentacja + testowanie):**
- Auth: http://localhost:5101/swagger
- Tickets: http://localhost:5102/swagger
- Users: http://localhost:5103/swagger

**Pliki `.http` (opcjonalnie - dla VS Code REST Client):**
- `src/AuthService/AuthService.http`
- `src/TicketService/TicketService.http`
- `src/UserService/UserService.http`

> **Frontend powinien używać wyłącznie API Gateway (port 5100)!**

> **Uwaga:** Kontenery używają HTTP (port 8080). W production AWS ALB obsługuje HTTPS 
> i przekazuje ruch do kontenerów przez HTTP w prywatnej sieci VPC.

## 📚 Technologie

- **.NET 9.0** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **RabbitMQ** - Message Broker
- **Ocelot** - API Gateway
- **Docker** - Containerization
- **AWS** - Cloud Platform
- **CloudFormation** - Infrastructure as Code

## 👥 PROJEKT

Ten projekt demonstruje:
- ✅ Architekturę mikroservisową
- ✅ System rozproszony
- ✅ Event-driven communication
- ✅ Database per service pattern
- ✅ API Gateway pattern
- ✅ Containerization
- ✅ Cloud-ready deployment
- ✅ Separation of concerns
- ✅ Scalability i resilience

