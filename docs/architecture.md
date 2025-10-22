# Architektura Systemu Helpdesk

## Diagram Architektury

```
                                    ┌─────────────────┐
                                    │   API Gateway   │
                                    │    (Ocelot)     │
                                    │   Port: 5000    │
                                    └────────┬────────┘
                                             │
                        ┌────────────────────┼────────────────────┐
                        │                    │                    │
                   ┌────▼─────┐      ┌──────▼──────┐      ┌──────▼──────┐
                   │   Auth   │      │   Ticket    │      │    User     │
                   │  Service │      │   Service   │      │   Service   │
                   │Port: 5001│      │ Port: 5002  │      │ Port: 5003  │
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
                                    │  Port: 5004      │
                                    └──────────────────┘
```

## TODO: Szczegóły do dodania
- Sequence diagrams dla głównych przepływów
- Component diagram
- Deployment diagram dla AWS
- Database schemas
