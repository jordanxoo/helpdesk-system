# 🗄️ Database Expansion - Dokumentacja Rozbudowy Bazy Danych

> **Branch:** `database-expansion`  
> **Data rozpoczęcia:** 2025-11-12  
> **Status:** ✅ Faza 1 Zakończona

---

## 📋 Spis Treści
1. [Przegląd Zmian](#przegląd-zmian)
2. [Nowe Encje](#nowe-encje)
3. [Relacje Między Tabelami](#relacje-między-tabelami)
4. [API Endpoints](#api-endpoints)
5. [Testy](#testy)
6. [Następne Kroki](#następne-kroki)

---

## 🎯 Przegląd Zmian

### Cel Rozbudowy
Przekształcenie TicketService z prostego systemu zgłoszeń w centrum zarządzania logiką biznesową z pełnym wsparciem dla:
- Klientów korporacyjnych (Organizations)
- Umów serwisowych (SLA)
- Elastycznej kategoryzacji (Tags)
- Załączników do zgłoszeń
- Pełnego śladu audytowego

### Co Zostało Dodane?
- ✅ **6 nowych tabel** w bazie `helpdesk_tickets`
- ✅ **5 nowych modeli** domenowych w `Shared/Models`
- ✅ **3 nowe kontrolery** API z autoryzacją
- ✅ **1 migracja EF Core** z pełną konfiguracją relacji
- ✅ **JSON Serialization** z obsługą circular references

---

## 🏗️ Nowe Encje

### 1. **SLA (Service Level Agreement)**
Umowa serwisowa definiująca czasy reakcji i rozwiązania zgłoszeń.

```csharp
public class SLA
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // Czasy reakcji (w minutach) dla różnych priorytetów
    public int ResponseTimeCritical { get; set; } = 60;    // 1h
    public int ResponseTimeHigh { get; set; } = 240;        // 4h
    public int ResponseTimeMedium { get; set; } = 480;      // 8h
    public int ResponseTimeLow { get; set; } = 1440;        // 24h
    
    // Czasy rozwiązania (w minutach)
    public int ResolutionTimeCritical { get; set; } = 240;  // 4h
    public int ResolutionTimeHigh { get; set; } = 480;      // 8h
    public int ResolutionTimeMedium { get; set; } = 1440;   // 1 dzień
    public int ResolutionTimeLow { get; set; } = 4320;      // 3 dni
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ICollection<Organization> Organizations { get; set; }
}
```

**Tabela:** `slas`

**Zastosowanie:**
- Definiowanie standardów obsługi dla różnych klientów
- Automatyczne obliczanie deadline'ów dla zgłoszeń
- Raportowanie breach'ów SLA

---

### 2. **Organization**
Organizacja/Klient korporacyjny - kontener dla wielu użytkowników.

```csharp
public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // FK do SLA
    public Guid? SlaId { get; set; }
    public SLA? Sla { get; set; }
    
    public ICollection<Ticket> Tickets { get; set; }
}
```

**Tabela:** `organizations`

**Zastosowanie:**
- Grupowanie użytkowników z tej samej firmy
- Przypisywanie dedykowanych SLA do organizacji
- Raportowanie per organizacja

---

### 3. **Tag**
Elastyczna etykieta do kategoryzacji zgłoszeń (Many-to-Many z Ticket).

```csharp
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }          // UNIQUE
    public string? Color { get; set; }        // hex, np. "#FF5733"
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Ticket> Tickets { get; set; }
}
```

**Tabela:** `tags`  
**Tabela łącząca:** `ticket_tags` (ticket_id, tag_id)

**Zastosowanie:**
- Dynamiczna kategoryzacja zgłoszeń (np. "bug", "urgent", "security")
- Filtrowanie i wyszukiwanie
- Kolorowe oznaczenia w UI

---

### 4. **TicketAttachment**
Załącznik do zgłoszenia - metadata (plik fizyczny w S3/storage).

```csharp
public class TicketAttachment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UploadedById { get; set; }
    
    public string FileName { get; set; }
    public string ContentType { get; set; }      // MIME type
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; }      // S3 key / file path
    public string? DownloadUrl { get; set; }     // Pre-signed URL
    
    public DateTime UploadedAt { get; set; }
    
    public Ticket? Ticket { get; set; }
}
```

**Tabela:** `ticket_attachments`

**Zastosowanie:**
- Przechowywanie screenshotów, logów, dokumentów
- Metadata w bazie, pliki w S3/storage
- Gotowe do integracji z AWS S3

---

### 5. **TicketAuditLog**
Pełny ślad audytowy wszystkich zmian w zgłoszeniu.

```csharp
public class TicketAuditLog
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    
    public AuditAction Action { get; set; }    // Enum: Created, Updated, StatusChanged...
    public string? FieldName { get; set; }      // Nazwa zmienionego pola
    public string? OldValue { get; set; }       // JSON
    public string? NewValue { get; set; }       // JSON
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Ticket? Ticket { get; set; }
}

public enum AuditAction
{
    Created, Updated, StatusChanged, PriorityChanged,
    Assigned, Unassigned, CommentAdded, AttachmentAdded,
    AttachmentRemoved, Closed, Reopened
}
```

**Tabela:** `ticket_audit_logs`

**Zastosowanie:**
- Compliance i audyty
- Historia zmian dla każdego zgłoszenia
- Debugging (kto, kiedy, co zmienił)

---

## 🔗 Relacje Między Tabelami

### Diagram Relacji

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│     SLA     │◄────────│ Organization │◄────────│   Ticket    │
│             │ 1     * │              │ 1     * │             │
└─────────────┘         └──────────────┘         └─────────────┘
                                                        │
                                                        │ 1
                                                        │
                        ┌───────────────────────────────┼───────────────┐
                        │                               │               │
                        │ *                             │ *             │ *
                  ┌─────▼─────┐                   ┌────▼────┐    ┌─────▼──────┐
                  │    Tag    │                   │ Comment │    │ Attachment │
                  │           │                   │         │    │            │
                  └───────────┘                   └─────────┘    └────────────┘
                        ▲                               
                        │ *                             
                        │                               
                  ┌─────┴──────┐                  ┌──────────────┐
                  │ticket_tags │                  │  AuditLog    │
                  │(join table)│                  │              │
                  └────────────┘                  └──────────────┘
```

### Szczegóły Relacji

#### **Ticket → Organization (Many-to-One, Optional)**
- FK: `Ticket.OrganizationId` → `Organization.Id`
- CASCADE: `SetNull` (usunięcie organizacji nie usuwa ticketów)
- **Cel:** Zgłoszenia korporacyjne vs. indywidualne

#### **Ticket → SLA (Many-to-One, Optional)**
- FK: `Ticket.SlaId` → `SLA.Id`
- CASCADE: `SetNull`
- **Cel:** "Zamrożenie" SLA w momencie utworzenia ticketa

#### **Organization → SLA (Many-to-One, Optional)**
- FK: `Organization.SlaId` → `SLA.Id`
- CASCADE: `SetNull`
- **Cel:** Każda organizacja ma przypisane SLA

#### **Ticket ↔ Tag (Many-to-Many)**
- Tabela łącząca: `ticket_tags`
- FK: `ticket_tags.ticket_id` → `Ticket.Id` (CASCADE)
- FK: `ticket_tags.tag_id` → `Tag.Id` (CASCADE)
- **EF Core:** `.UsingEntity("ticket_tags")`

#### **Ticket → TicketAttachment (One-to-Many)**
- FK: `TicketAttachment.TicketId` → `Ticket.Id`
- CASCADE: `Cascade` (usunięcie ticketa usuwa załączniki)

#### **Ticket → TicketAuditLog (One-to-Many)**
- FK: `TicketAuditLog.TicketId` → `Ticket.Id`
- CASCADE: `Cascade` (usunięcie ticketa usuwa audit logi)

---

## 🔌 API Endpoints

### 1. **SLA Management** (Admin Only)

```http
GET    /api/slas?page=1&pageSize=20&activeOnly=false
GET    /api/slas/{id}
POST   /api/slas
PUT    /api/slas/{id}
DELETE /api/slas/{id}
```

**Autoryzacja:** `[Authorize(Roles = "Administrator")]`

**Przykład Create:**
```json
POST /api/slas
{
  "name": "Premium SLA",
  "description": "VIP customers - 24/7 support",
  "responseTimeCritical": 30,
  "responseTimeHigh": 120,
  "responseTimeMedium": 240,
  "responseTimeLow": 480,
  "resolutionTimeCritical": 120,
  "resolutionTimeHigh": 240,
  "resolutionTimeMedium": 480,
  "resolutionTimeLow": 1440
}
```

---

### 2. **Organization Management** (Admin Only)

```http
GET    /api/organizations?page=1&pageSize=20&activeOnly=false
GET    /api/organizations/{id}
POST   /api/organizations
PUT    /api/organizations/{id}
DELETE /api/organizations/{id}
```

**Autoryzacja:** `[Authorize(Roles = "Administrator")]`

**Przykład Create:**
```json
POST /api/organizations
{
  "name": "Acme Corporation",
  "description": "Large enterprise customer",
  "contactEmail": "support@acme.com",
  "contactPhone": "+1-555-0123",
  "slaId": "e6a80e28-06b0-44bb-b026-a42b15ab28af"
}
```

---

### 3. **Tag Management** (Agent/Admin)

```http
GET    /api/tags?search=urgent
GET    /api/tags/{id}
POST   /api/tags          # Admin only
PUT    /api/tags/{id}     # Admin only
DELETE /api/tags/{id}     # Admin only
```

**Autoryzacja:** 
- GET: `[Authorize(Roles = "Agent,Administrator")]`
- POST/PUT/DELETE: `[Authorize(Roles = "Administrator")]`

**Przykład Create:**
```json
POST /api/tags
{
  "name": "security",
  "color": "#FF0000",
  "description": "Security-related issues"
}
```

**Walidacja:**
- ✅ Tag names muszą być unique
- ✅ Color w formacie hex (#RRGGBB)

---

## 🧪 Testy

### Testy Manualne - Wykonane ✅

#### 1. **Utworzenie SLA**
```bash
curl -X POST http://localhost:5102/api/slas \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name": "Standard SLA", ...}'
```
**Rezultat:** ✅ SLA utworzone z ID: `e6a80e28-06b0-44bb-b026-a42b15ab28af`

#### 2. **Utworzenie Organization z SLA**
```bash
curl -X POST http://localhost:5102/api/organizations \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"name": "Acme Corporation", "slaId": "..."}'
```
**Rezultat:** ✅ Organization utworzona, SLA poprawnie powiązane

#### 3. **Utworzenie Tagów**
```bash
curl -X POST http://localhost:5102/api/tags \
  -d '{"name": "urgent", "color": "#FF0000"}'
```
**Rezultat:** ✅ 2 tagi utworzone (urgent, bug)

#### 4. **Pobieranie z Relacjami**
```bash
curl http://localhost:5102/api/organizations
```
**Rezultat:** ✅ JSON z zagnieżdżonym SLA (circular reference handled)

#### 5. **Weryfikacja w Bazie**
```sql
SELECT * FROM slas;
SELECT * FROM organizations;
SELECT * FROM tags;
```
**Rezultat:** ✅ Wszystkie dane w PostgreSQL

---

## 🐛 Napotkane Problemy i Rozwiązania

### Problem 1: JSON Circular Reference
**Error:**
```
System.Text.Json.JsonException: A possible object cycle was detected
Path: $.data.Sla.Organizations.Sla.Organizations...
```

**Przyczyna:**
Organization → SLA → Organizations → SLA → ... (nieskończona pętla)

**Rozwiązanie:**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
```

✅ **Fixed!**

---

## 💡 Ważne na Przyszłość (Backlog)

### Attachments & File Storage
- [ ] Upload plików przez API (multipart/form-data)
- [ ] Integracja z AWS S3 lub lokalnym storage
- [ ] Pre-signed URLs dla bezpiecznego download'u
- [ ] Walidacja typów i rozmiaru plików

### Audit Trail - Automatyzacja
- [ ] Interceptor/Middleware do automatycznego logowania zmian
- [ ] Event handler dla wszystkich operacji na ticketach
- [ ] Endpoint GET `/api/tickets/{id}/history` do przeglądania zmian

### UserService Integration
- [ ] Dodanie `OrganizationId` do modelu User
- [ ] RabbitMQ Events: `UserJoinedOrganization`, `UserLeftOrganization`
- [ ] Synchronizacja między AuthService i UserService

---

## 📊 Statystyki

- **Nowe pliki:** 15+
- **Zmodyfikowane pliki:** 5
- **Linie kodu:** ~2000
- **Tabele w bazie:** 9 (było 3)
- **API endpoints:** 18 (było 12)
- **Czas implementacji:** ~2h

---

## 📝 Notatki Techniczne

### Best Practices Zastosowane
- ✅ **Repository Pattern** - separacja logiki dostępu do danych
- ✅ **Industry Standard Naming** - snake_case dla kolumn PostgreSQL
- ✅ **Proper Indexing** - indeksy na FK i często używanych polach
- ✅ **Cascade Behavior** - przemyślane CASCADE/SET NULL
- ✅ **DTO Pattern** - separacja modeli domenowych od API contracts
- ✅ **Role-Based Authorization** - bezpieczeństwo endpointów

### Nie Przesadziliśmy (No Overkill)
- ❌ Stored Procedures (nie potrzebne dla studenckiego projektu)
- ❌ Materialized Views (przedwczesna optymalizacja)
- ❌ CQRS Pattern (za dużo complexity)
- ❌ Event Sourcing (overkill dla tej skali)

---

**Ostatnia aktualizacja:** 2025-11-12 21:10 CET  
**Status:** ✅ Faza 1 Zakończona - Wszystko Działa!
