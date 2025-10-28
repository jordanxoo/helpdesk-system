# Helpdesk System - System Rozproszonej Pomocy Technicznej

System Helpdesk zbudowany jako rozproszona aplikacja mikroservisowa, gotowa do deploymentu na AWS.

## 🏗️ Architektura

System składa się z następujących mikroservisów:

### Mikroservisy
- **AuthService** (Port 5101) - Uwierzytelnianie i autoryzacja użytkowników (JWT, Identity)
- **TicketService** (Port 5102) - Zarządzanie zgłoszeniami helpdesk
- **UserService** (Port 5103) - Zarządzanie profilami użytkowników
- **NotificationService** (Port 5104) - Wysyłanie powiadomień (email, SMS)
- **ApiGateway** (Port 5100) - Ocelot API Gateway dla routingu żądań

### Infrastruktura
- **PostgreSQL** - Bazy danych dla każdego serwisu
- **RabbitMQ** - Message broker dla komunikacji asynchronicznej między serwisami
- **Docker & Docker Compose** - Konteneryzacja i orkiestracja lokalna
- **AWS ECS/Fargate** - Deployment w chmurze (CloudFormation templates)

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
# Zbudowanie i uruchomienie wszystkich serwisów
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

- **API Gateway**: http://localhost:5100
- **Auth Service**: http://localhost:5101
- **Ticket Service**: http://localhost:5102
- **User Service**: http://localhost:5103
- **Notification Service**: http://localhost:5104
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

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

## 📊 Message Queue

System używa RabbitMQ dla komunikacji event-driven między serwisami:

### Queues:
- `ticket-created` - Nowy ticket utworzony
- `ticket-assigned` - Ticket przypisany do agenta
- `ticket-status-changed` - Status ticketu zmieniony
- `comment-added` - Komentarz dodany
- `user-registered` - Nowy użytkownik zarejestrowany
- `send-email` - Wysyłka emaila
- `send-sms` - Wysyłka SMS

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

Każdy serwis ma plik `.http` z przykładowymi requestami:
- `src/AuthService/AuthService.http` - Rejestracja, logowanie, refresh token
- Otwórz w VS Code i kliknij "Send Request" (wymaga REST Client extension)

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

