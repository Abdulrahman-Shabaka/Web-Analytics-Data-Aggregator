# Requirements and Bonus Tasks - Web Analytics Data Aggregator

## 📋 Core Requirements

### Step 1: Read Data (Ingestion)
- [ ] Create two adapters/services that read JSON files
  - Google Analytics (GA) mock format:
    ```json
    {
      "date": "2025-10-20",
      "page": "/home",
      "users": 120,
      "sessions": 150,
      "views": 310
    }
    ```
  - PageSpeed Insights (PSI) mock format:
    ```json
    {
      "date": "2025-10-20",
      "page": "/home",
      "performanceScore": 0.9,
      "LCP_ms": 2100
    }
    ```
- [ ] Combine GA and PSI data into a standard record:
  ```json
  {
    "page": "/home",
    "date": "2025-10-20",
    "users": 120,
    "sessions": 150,
    "views": 310,
    "performanceScore": 0.9,
    "LCP_ms": 2100
  }
  ```

### Step 2: Publish to Real Message Broker (MANDATORY)
- [ ] Use RabbitMQ or Apache Kafka (in-memory queues are NOT allowed)
- [ ] Publish each combined record to broker:
  - **Kafka**: topic `analytics.raw`
  - **RabbitMQ**: exchange `analytics.raw` (fanout/direct) and queue `analytics.raw.q`

### Step 3: Process & Aggregate (Background Consumer)
- [ ] Create a background worker that consumes from the broker
- [ ] Aggregate data per day:
  - `totalUsers` (sum)
  - `totalSessions` (sum)
  - `totalViews` (sum)
  - `avgPerformance` (mean of performanceScore)
- [ ] Persist aggregated data with EF Core to SQL database
- [ ] Aggregated example format:
  ```json
  {
    "date": "2025-10-20",
    "totalUsers": 480,
    "totalSessions": 550,
    "totalViews": 1200,
    "avgPerformance": 0.88
  }
  ```
- [ ] Basic reliability:
  - Acknowledge messages only after successful save
  - Include simple retry mechanism (3 attempts with backoff)

### Step 4: Reporting APIs (JWT-Protected)
- [ ] `GET /reports/overview` → totals across all pages & dates
- [ ] `GET /reports/pages` → grouped by page (with per-page totals/averages)
- [ ] Authentication:
  - Basic email/password signup/login
  - JWT (Bearer token) for report endpoints

### Database Design
- [ ] **Users Table**:
  - Id
  - Name
  - Email
  - PasswordHash
  - CreatedAt

- [ ] **RawData Table**:
  - Id
  - Date
  - Page
  - Users
  - Sessions
  - Views
  - PerformanceScore
  - LCPms
  - ReceivedAt

- [ ] **DailyStats Table**:
  - Id
  - Date
  - TotalUsers
  - TotalSessions
  - TotalViews
  - AvgPerformance
  - LastUpdatedAt

### Acceptance Checks (MANDATORY)
- [ ] Docker Compose that starts:
  - API
  - Database
  - Message Broker (RabbitMQ or Kafka)
- [ ] Producer service that publishes to the real broker
- [ ] Consumer background service that reads from broker and writes to DB
- [ ] Clear logs showing:
  - Messages published → consumed → saved
  - Retry attempts on transient failures
- [ ] Swagger showing the secured report endpoints
- [ ] Quick seed script or pre-bundled JSON mock files to generate sample data

### Tech Stack Requirements
- [ ] Backend: .NET 8 + ASP.NET Core Web API
- [ ] Database: SQL Server or PostgreSQL (EF Core)
- [ ] Message Broker: RabbitMQ or Apache Kafka (REQUIRED)
- [ ] Authentication: JWT
- [ ] Documentation: Swagger / OpenAPI
- [ ] Runtime: Docker Compose

### Submission Requirements
- [ ] GitHub Repository with:
  - Organized code (API, Services, Data, Models, Background worker)
  - `docker-compose.yml` (API + DB + Broker)
  - `README.md` with setup steps:
    - `docker compose up -d`
    - How to seed/read JSON
    - How to hit Swagger + get a JWT
  - Optional `ARCHITECTURE.md` (flow diagram & key decisions)
- [ ] 10-Minute Video:
  - 3 min — Introduce yourself + two technical challenges you've solved
  - 7 min — Show the end-to-end flow (publish → consume → DB → API demo)

---

## 🎁 Bonus Points (Optional)

- [ ] Docker Compose healthchecks & wait-for scripts
- [ ] Dead-letter queue (DLQ) with reason captured
  - RabbitMQ: `analytics.dlq`
- [ ] Unit tests (adapters, aggregator)
- [ ] Minimal frontend page to display reports
- [ ] Metrics endpoint (e.g., `/health`, `/metrics`)
- [ ] README diagram of flow (Producer → Broker → Consumer → DB → API)

---

## 📊 Evaluation Criteria

- **Code Quality & Structure** (clean architecture, separation of concerns) - 30%
- **Completeness & Correctness** (end-to-end through the real broker) - 25%
- **API Design & Documentation** (clear models, Swagger, auth) - 20%
- **Database Design** (sensible schema, EF migrations) - 15%
- **Reliability & Error Handling** (retries, acks, idempotency basics) - 10%
- **Bonus**: Docker polish, DLQ, tests, simple UI, metrics

---

## 📝 Notes

- Queue is **MANDATORY** - in-memory queues are not allowed
- You do NOT need real GA/PSI APIs - mock with JSON files
- All report endpoints must be JWT-protected
- Must use a real message broker (RabbitMQ or Kafka)
- Docker Compose must start all required services

