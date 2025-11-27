# Architektura Systemu Helpdesk

## Diagram Architektury

```
                                    ┌─────────────────┐
                                    │   API Gateway   │
                                    │    (Ocelot)     │
                                    │   Port: 5100    │
                                    └────────┬────────┘
                                             │
                        ┌────────────────────┼────────────────────┐
                        │                    │                    │
                   ┌────▼─────┐      ┌──────▼──────┐      ┌──────▼──────┐
                   │   Auth   │      │   Ticket    │      │    User     │
                   │  Service │      │   Service   │      │   Service   │
                   │Port: 5101│      │ Port: 5102  │      │ Port: 5103  │
                   └────┬─────┘      └──────┬──────┘      └──────┬──────┘
                        │                   │                     │
                        │                   │                     │
                   ┌────▼─────┐      ┌──────▼──────┐      ┌──────▼──────┐
                   │  Auth DB │      │ Tickets DB  │      │  Users DB   │
                   │PostgreSQL│      │ PostgreSQL  │      │ PostgreSQL  │
                   └──────────┘      └─────────────┘      └─────────────┘
                                             │
                                             │
                                    ┌────────▼─────────┐
                                    │    RabbitMQ      │
                                    │ Message Broker   │
                                    │  Port: 5672      │
                                    └────────┬─────────┘
                                             │
                                    ┌────────▼─────────┐
                                    │  Notification    │
                                    │    Service       │
                                    │  Port: 5104      │
                                    └──────────────────┘
```

## Bezpieczeństwo i HTTPS

### SSL/TLS Termination

System **nie używa** `UseHttpsRedirection()` w aplikacjach, ponieważ:

#### Development/Docker
- Kontenery komunikują się przez **HTTP** w sieci Docker
- Brak potrzeby szyfrowania ruchu w localhost
- Prostsze debugowanie i development

#### Production/AWS
- **AWS Application Load Balancer (ALB)** obsługuje SSL/TLS termination
- Certyfikat SSL zarządzany przez AWS Certificate Manager (ACM)
- Kontenery w ECS/Fargate otrzymują ruch przez **HTTP** w prywatnej sieci VPC

```
Internet (HTTPS :443)
    ↓
AWS ALB + SSL Certificate
    ↓ (SSL Termination)
    ↓ HTTP :8080 (VPC - bezpieczna prywatna sieć)
    ↓
ECS Tasks (Containers)
```

**Zalety tego podejścia:**
- ✅ Centralne zarządzanie certyfikatami (ACM)
- ✅ Automatyczne odnowienie certyfikatów
- ✅ Lepsza wydajność (kontenery nie muszą obsługiwać SSL)
- ✅ Zgodne z best practices dla mikroservisów
- ✅ Brak problemów z redirect loops

### Konfiguracja JWT

```csharp
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Wyłączone dla SSL termination na ALB
    // ...
});
```

`RequireHttpsMetadata = false` pozwala na walidację tokenów JWT przez HTTP, 
ponieważ ruch HTTPS jest już obsłużony przez ALB.

## Clean Separation: AuthService vs UserService

### Podział odpowiedzialności

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         REJESTRACJA UŻYTKOWNIKA                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Frontend ──POST /register──▶ AuthService                               │
│                                    │                                    │
│                         1. Twórz credentials (email, hasło)             │
│                         2. Generuj JWT token                            │
│                         3. Publikuj UserRegisteredEvent ──▶ RabbitMQ    │
│                                    │                                    │
│                                    │ (fail-safe: jeśli RabbitMQ error   │
│                                    │  → rollback user → return 500)     │
│                                    │                                    │
│                              RabbitMQ                                   │
│                                    │                                    │
│                                    ▼                                    │
│                              UserService                                │
│                                    │                                    │
│                         4. Twórz profil (imię, nazwisko, telefon, rola) │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Co gdzie jest przechowywane?

| Dane | AuthService (helpdesk_auth) | UserService (helpdesk_users) |
|------|----------------------------|------------------------------|
| Email | Login (credential) | Dana kontaktowa |
| Hasło | PasswordHash | - |
| Refresh Token | Tak | - |
| Imię, Nazwisko | - | Tak |
| Telefon | - | Tak |
| Rola | Identity (authorization) | Biznesowa (assignment) |
| Organizacja | - | Tak |
| IsActive | - | Tak |

### Dlaczego Email i Rola są w obu miejscach?

**To świadoma decyzja architektoniczna, nie błąd:**

- **Email w AuthService** = credential do logowania
- **Email w UserService** = dana kontaktowa (do wysyłki powiadomień)

- **Rola w AuthService** = uprawnienie (czy mogę wejść?)
- **Rola w UserService** = funkcja biznesowa (czy można mi przypisać ticket?)

### Fail-safe Registration

Rejestracja jest **atomowa** - zapobiega desync między serwisami:

```
1. AuthService tworzy usera w bazie
2. AuthService publikuje event do RabbitMQ
   ├── SUCCESS → zwróć token do frontendu
   └── FAILURE → usuń usera z bazy → zwróć 500
```

Dzięki temu nigdy nie wystąpi sytuacja gdzie user istnieje w AuthService ale nie ma go w UserService.
