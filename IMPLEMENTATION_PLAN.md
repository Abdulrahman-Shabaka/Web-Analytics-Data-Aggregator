# 🚀 2-Hour Implementation Plan - Web Analytics Data Aggregator

## ⏱️ Time Allocation Strategy

**Total: 120 minutes**
- **Phase 1: Project Setup & Database (20 min)**
- **Phase 2: Data Ingestion & Message Broker (25 min)**
- **Phase 3: Background Consumer & Aggregation (25 min)**
- **Phase 4: JWT Auth & Reporting APIs (25 min)**
- **Phase 5: Docker Compose & Testing (20 min)**
- **Buffer: 5 min** (for unexpected issues)

---

## 📋 Phase 1: Project Setup & Database (20 min)

### 1.1 Create Solution Structure (5 min)
```
AnalyticsAggregator/
├── src/
│   ├── AnalyticsAggregator.API/          # Web API project
│   ├── AnalyticsAggregator.Core/         # Domain models & interfaces
│   ├── AnalyticsAggregator.Infrastructure/ # EF Core, RabbitMQ, services
│   └── AnalyticsAggregator.Worker/       # Background consumer service
├── Data/
│   ├── MockData/
│   │   ├── ga_data.json
│   │   └── psi_data.json
├── docker-compose.yml
└── README.md
```

**Actions:**
- Create .NET 8 solution with 4 projects
- Add NuGet packages:
  - `Microsoft.EntityFrameworkCore.SqlServer` or `Npgsql.EntityFrameworkCore.PostgreSQL`
  - `RabbitMQ.Client`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `BCrypt.Net-Next` (password hashing)
  - `Swashbuckle.AspNetCore` (Swagger)

### 1.2 Database Models & EF Core Setup (10 min)
- Create entities: `User`, `RawData`, `DailyStats`
- Create `ApplicationDbContext`
- Configure relationships and indexes
- Create initial migration

### 1.3 Seed Data JSON Files (5 min)
- Create sample `ga_data.json` (3-5 records)
- Create sample `psi_data.json` (matching pages/dates)
- Place in `Data/MockData/` folder

---

## 📋 Phase 2: Data Ingestion & Message Broker (25 min)

### 2.1 Data Adapters (10 min)
- `IGoogleAnalyticsAdapter` + implementation
  - Read JSON files from `Data/MockData/ga_data.json`
  - Parse and return `List<GoogleAnalyticsRecord>`
- `IPageSpeedInsightsAdapter` + implementation
  - Read JSON files from `Data/MockData/psi_data.json`
  - Parse and return `List<PageSpeedInsightsRecord>`
- `IDataCombiner` service
  - Combine GA + PSI by `date` and `page`
  - Return `List<CombinedAnalyticsRecord>`

### 2.2 RabbitMQ Setup (10 min)
- Create `IRabbitMQService` interface
- Implement `RabbitMQService`:
  - Connection factory setup
  - Exchange: `analytics.raw` (direct)
  - Queue: `analytics.raw.q`
  - Publish method with JSON serialization
- Register in DI container

### 2.3 Producer Service (5 min)
- Create `IDataIngestionService`
- Implement:
  - Read from adapters
  - Combine data
  - Publish each record to RabbitMQ
- Add endpoint: `POST /api/ingestion/start` (trigger ingestion)

---

## 📋 Phase 3: Background Consumer & Aggregation (25 min)

### 3.1 Background Worker Setup (5 min)
- Create `AnalyticsConsumerWorker` (IHostedService)
- Setup RabbitMQ consumer connection
- Subscribe to `analytics.raw.q`

### 3.2 Message Processing (10 min)
- Deserialize incoming messages
- Save to `RawData` table (EF Core)
- Implement retry logic (3 attempts with exponential backoff)
- Acknowledge only after successful save
- Log all operations (published → consumed → saved)

### 3.3 Aggregation Logic (10 min)
- Create `IAggregationService`
- Implement daily aggregation:
  - Group by `date`
  - Sum: `totalUsers`, `totalSessions`, `totalViews`
  - Average: `avgPerformance`
- Upsert to `DailyStats` table (update if exists, insert if new)
- Call aggregation after each successful RawData save

---

## 📋 Phase 4: JWT Auth & Reporting APIs (25 min)

### 4.1 User Authentication (10 min)
- Create `IUserService` with:
  - `RegisterAsync(email, password, name)`
  - `LoginAsync(email, password)` → returns JWT
- Implement password hashing (BCrypt)
- Configure JWT in `Program.cs`:
  - Secret key
  - Token expiration
  - Bearer authentication scheme

### 4.2 Auth Endpoints (5 min)
- `POST /api/auth/register`
- `POST /api/auth/login` → returns JWT token

### 4.3 Reporting APIs (10 min)
- Create `ReportsController` with `[Authorize]`
- `GET /api/reports/overview`:
  - Aggregate all `DailyStats`
  - Return totals across all dates
- `GET /api/reports/pages`:
  - Query `RawData` grouped by `page`
  - Return per-page totals and averages
- Add Swagger configuration with JWT support

---

## 📋 Phase 5: Docker Compose & Testing (20 min)

### 5.1 Dockerfile for API (5 min)
- Multi-stage build
- Expose port 8080
- Copy and run migrations on startup

### 5.2 docker-compose.yml (10 min)
```yaml
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: analytics
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    healthcheck: ...

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    healthcheck: ...

  api:
    build: .
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
    ports:
      - "8080:8080"
    environment:
      ConnectionStrings__DefaultConnection: ...
      RabbitMQ__HostName: rabbitmq
```

### 5.3 README.md (5 min)
- Setup instructions
- How to run `docker compose up -d`
- How to trigger ingestion
- How to get JWT and test endpoints
- Swagger URL

---

## 🎁 Bonus Features (If Time Permits)

### Priority 1 (if 10+ min left):
- ✅ Dead-letter queue (DLQ) setup
- ✅ Health check endpoint (`/health`)

### Priority 2 (if 20+ min left):
- ✅ Docker healthchecks & wait-for scripts
- ✅ README diagram (ASCII art flow)

### Priority 3 (if 30+ min left):
- ✅ Unit tests (1-2 key services)
- ✅ Metrics endpoint

---

## 🔧 Technical Decisions

### Message Broker: **RabbitMQ**
- ✅ Simpler setup than Kafka
- ✅ Good .NET client support
- ✅ Management UI included
- ✅ Easier DLQ implementation

### Database: **PostgreSQL**
- ✅ Lightweight Docker image
- ✅ Good EF Core support
- ✅ Free and open-source

### Architecture: **Clean Architecture Lite**
- API layer (controllers, DTOs)
- Core layer (domain models, interfaces)
- Infrastructure layer (EF Core, RabbitMQ, services)
- Worker layer (background consumer)

### Key NuGet Packages:
```
- Microsoft.EntityFrameworkCore.PostgreSQL
- RabbitMQ.Client
- Microsoft.AspNetCore.Authentication.JwtBearer
- BCrypt.Net-Next
- Swashbuckle.AspNetCore
- Serilog.AspNetCore (logging)
```

---

## ✅ Acceptance Checklist

- [ ] Docker Compose starts all services
- [ ] Producer publishes to RabbitMQ
- [ ] Consumer reads from RabbitMQ and saves to DB
- [ ] Aggregation works correctly
- [ ] JWT auth protects report endpoints
- [ ] Swagger shows secured endpoints
- [ ] Sample JSON files included
- [ ] Logs show: publish → consume → save flow
- [ ] Retry mechanism works
- [ ] README with clear instructions

---

## 🚨 Risk Mitigation

1. **RabbitMQ connection issues**: Use healthchecks and wait-for scripts
2. **Database migration failures**: Run migrations in startup code with retries
3. **Time pressure**: Focus on core requirements first, skip bonuses if needed
4. **JWT configuration**: Use simple symmetric key for speed
5. **Testing**: Manual testing via Swagger, skip unit tests if time is tight

---

## 📝 Quick Reference Commands

```bash
# Create solution
dotnet new sln -n AnalyticsAggregator
dotnet new webapi -n AnalyticsAggregator.API
dotnet new classlib -n AnalyticsAggregator.Core
dotnet new classlib -n AnalyticsAggregator.Infrastructure
dotnet new worker -n AnalyticsAggregator.Worker

# Add packages
dotnet add package Microsoft.EntityFrameworkCore.PostgreSQL
dotnet add package RabbitMQ.Client
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next

# Run
docker compose up -d
dotnet run --project src/AnalyticsAggregator.API
```

---

## 🎯 Success Criteria

**Minimum Viable Product (MVP):**
1. ✅ All 4 phases complete
2. ✅ End-to-end flow works: JSON → RabbitMQ → DB → API
3. ✅ JWT auth functional
4. ✅ Docker Compose runs everything
5. ✅ Swagger accessible

**Nice to Have:**
- DLQ implementation
- Health checks
- Better error handling
- Unit tests

---

**Let's start building! 🚀**

