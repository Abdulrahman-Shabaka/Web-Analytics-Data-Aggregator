# Web Analytics Data Aggregator

A .NET 8 application that aggregates web analytics data from Google Analytics and PageSpeed Insights, processes it through RabbitMQ, and provides JWT-protected reporting APIs.

## 🏗️ Architecture

```
┌─────────────┐
│  JSON Files │
│  (GA + PSI) │
└──────┬──────┘
       │
       ▼
┌─────────────┐     ┌──────────────┐
│   Adapters  │────▶│   Combiner   │
│  (GA + PSI) │     │   Service    │
└─────────────┘     └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
                    │   RabbitMQ   │
                    │  Exchange:   │
                    │ analytics.raw│
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
                    │    Queue:    │
                    │ analytics.   │
                    │   raw.q      │
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐     ┌──────────────┐
                    │   Worker     │────▶│  PostgreSQL  │
                    │  Consumer    │     │   Database   │
                    │  (Retry +    │     │ (RawData +   │
                    │   DLQ)       │     │ DailyStats)  │
                    └──────────────┘     └──────┬───────┘
                                                 │
                                                 ▼
                                          ┌──────────────┐
                                          │  REST API    │
                                          │  (JWT Auth)  │
                                          └──────┬───────┘
                                                 │
                                                 ▼
                                          ┌──────────────┐
                                          │   Frontend   │
                                          │   Dashboard  │
                                          └──────────────┘
```

## 📋 Features

- **Data Ingestion**: Reads GA and PSI data from JSON files
- **Message Broker**: Uses RabbitMQ for reliable message processing
- **Background Processing**: Worker service consumes messages and aggregates data
- **Database**: PostgreSQL with EF Core for data persistence
- **Authentication**: JWT-based authentication for API endpoints
- **Reporting**: RESTful APIs for analytics reports

## 🚀 Quick Start

### Prerequisites

- Docker and Docker Compose
- .NET 8 SDK (for local development)

### Running with Docker Compose

1. Clone the repository
2. Run the following command:

```bash
docker compose up -d
```

This will start:
- PostgreSQL database (port 5432)
- RabbitMQ with management UI (ports 5672, 15672)
- API service (port 8080)
- Worker service (background consumer)

### Accessing Services

- **API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Frontend Dashboard**: http://localhost:8080/index.html
- **Health Check**: http://localhost:8080/api/health
- **Metrics**: http://localhost:8080/api/health/metrics
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## 📖 Usage

### 1. Register a User

```bash
POST http://localhost:8080/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "name": "John Doe"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Login

```bash
POST http://localhost:8080/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### 3. Trigger Data Ingestion

```bash
POST http://localhost:8080/api/ingestion/start
```

This will:
- Read data from `Data/MockData/ga_data.json` and `Data/MockData/psi_data.json`
- Combine the data
- Publish each record to RabbitMQ
- Worker will consume and save to database

### 4. View Reports

#### Overview Report

```bash
GET http://localhost:8080/api/reports/overview
Authorization: Bearer <your-jwt-token>
```

#### Pages Report

```bash
GET http://localhost:8080/api/reports/pages
Authorization: Bearer <your-jwt-token>
```

## 📁 Project Structure

```
AnalyticsAggregator/
├── src/
│   ├── AnalyticsAggregator.API/          # Web API
│   ├── AnalyticsAggregator.Core/         # Domain models & interfaces
│   ├── AnalyticsAggregator.Infrastructure/ # EF Core, RabbitMQ, services
│   └── AnalyticsAggregator.Worker/       # Background consumer
├── Data/
│   └── MockData/
│       ├── ga_data.json                  # Google Analytics mock data
│       └── psi_data.json                 # PageSpeed Insights mock data
├── docker-compose.yml
└── README.md
```

## 🔧 Configuration

Environment variables can be set in `docker-compose.yml`:

- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `RabbitMQ__HostName`: RabbitMQ hostname
- `Jwt__Key`: JWT signing key (should be at least 32 characters)
- `DataPaths__GA`: Path to GA data file
- `DataPaths__PSI`: Path to PSI data file

## 🗄️ Database Schema

### Users
- Id, Name, Email, PasswordHash, CreatedAt

### RawData
- Id, Date, Page, Users, Sessions, Views, PerformanceScore, LCPms, ReceivedAt

### DailyStats
- Id, Date, TotalUsers, TotalSessions, TotalViews, AvgPerformance, LastUpdatedAt

## 🔐 Security

- Passwords are hashed using BCrypt
- JWT tokens expire after 24 hours
- Report endpoints require JWT authentication
- Swagger UI supports JWT authentication (click "Authorize" button)

## 📊 Data Flow

1. **Ingestion**: API endpoint triggers reading of JSON files
2. **Combination**: GA and PSI data are combined by date and page
3. **Publishing**: Each combined record is published to RabbitMQ exchange `analytics.raw`
4. **Consumption**: Worker service consumes messages from queue `analytics.raw.q`
5. **Processing**: 
   - Saves raw data to `RawData` table
   - Aggregates daily statistics
   - Updates `DailyStats` table
6. **Reporting**: API endpoints query aggregated data

## 🛠️ Development

### Running Locally

1. Start PostgreSQL and RabbitMQ:
```bash
docker compose up -d postgres rabbitmq
```

2. Update connection strings in `appsettings.json`

3. Run migrations:
```bash
dotnet ef database update --project src/AnalyticsAggregator.API
```

4. Run API:
```bash
dotnet run --project src/AnalyticsAggregator.API
```

5. Run Worker (in separate terminal):
```bash
dotnet run --project src/AnalyticsAggregator.Worker
```

## 🎁 Bonus Features Implemented

- ✅ **Docker Compose healthchecks** - PostgreSQL and RabbitMQ have healthchecks configured
- ✅ **Dead-letter queue (DLQ)** - Failed messages after 3 retries are sent to `analytics.dlq` with reason captured
- ✅ **Unit tests** - Test project created with sample tests for DataCombiner service
- ✅ **Minimal frontend page** - Dashboard at http://localhost:8080/index.html to display reports
- ✅ **Metrics endpoint** - `/api/health` and `/api/health/metrics` endpoints for monitoring
- ✅ **README diagram** - ASCII art flow diagram showing the complete architecture

## 📝 Notes

- The application uses in-memory database creation on startup (EnsureCreated)
- For production, use proper EF Core migrations
- RabbitMQ connection retries up to 10 times on startup
- Message processing retries 3 times with exponential backoff
- Messages are acknowledged only after successful database save

## 🐛 Troubleshooting

- **Database connection issues**: Ensure PostgreSQL is healthy before starting API/Worker
- **RabbitMQ connection issues**: Check RabbitMQ management UI at http://localhost:15672
- **JWT authentication fails**: Verify JWT key is set correctly
- **No data in reports**: Trigger ingestion endpoint and check worker logs

## 📄 License

This project is part of a hiring quest challenge.

