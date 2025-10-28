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

## TODO: Szczegóły do dodania
- Sequence diagrams dla głównych przepływów
- Component diagram
- Deployment diagram dla AWS
- Database schemas
