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

## TODO: Szczegóły do dodania
- Sequence diagrams dla głównych przepływów
- Component diagram
- Deployment diagram dla AWS
- Database schemas
