# IP Lookup Microservices Architecture

A comprehensive microservice-based system for IP address lookup and batch processing, built with .NET 9.0 and containerized with Docker.

## 📋 Project Overview

This project implements a scalable, enterprise-level microservices architecture consisting of three independent services that work together to provide IP address lookup functionality with caching and batch processing capabilities.

## 🏗️ Architecture

The system follows a microservices architecture with the following components:

```
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│  Batch Processor   │───▶│   IP Lookup        │───▶│   Cache Service     │
│   Service          │    │   Service           │    │                     │
│   (Port 5082)      │    │   (Port 5101)       │    │   (Port 5067)       │
└─────────────────────┘    └─────────────────────┘    └─────────────────────┘
                                    │                          │
                                    │                          │
                                    ▼                          ▼
                           ┌─────────────────────┐    ┌─────────────────────┐
                           │   External IP API   │    │   Memory Cache      │
                           │   (IPStack)         │    │   (1 min TTL)       │
                           └─────────────────────┘    └─────────────────────┘
```

## 🚀 Services

### 1. IP Lookup Microservice (Port 5101)

**Purpose**: Retrieves IP address details from external APIs with caching integration.

**Features**:
- RESTful API endpoint for single IP lookups
- Integration with IPStack API for external data
- Cache-first approach (checks cache before external API)
- Custom exception handling (`IPServiceNotAvailableException`)
- Input validation using FluentValidation
- Comprehensive error handling and logging

**Endpoints**:
- `GET /api/IpLookup/{ipAddress}` - Retrieve IP details

**Configuration**:
- `IpStack__ApiKey` - API key for IPStack service
- `ServiceUrls__CacheServiceBaseUrl` - Cache service URL

### 2. IP Detail Cache Microservice (Port 5067)

**Purpose**: Provides in-memory caching for IP address details to reduce external API calls.

**Features**:
- .NET MemoryCache implementation
- 1-minute expiration policy for cached entries
- RESTful endpoints for cache operations
- Automatic cache management
- High-performance in-memory storage

**Endpoints**:
- `GET /api/IpCache/{ipAddress}` - Retrieve cached IP details
- `POST /api/IpCache` - Store IP details in cache

**Configuration**:
- `CacheSettings__CacheExpiration` - Cache expiration time in minutes

### 3. Batch Processing Microservice (Port 5082)

**Purpose**: Handles asynchronous batch processing of multiple IP addresses.

**Features**:
- Asynchronous batch processing with background tasks
- Chunk-based processing (configurable batch size)
- Job tracking with unique GUIDs
- Real-time status monitoring
- Progress tracking and result storage
- Input validation and error handling

**Endpoints**:
- `POST /api/Batch` - Start batch processing
- `GET /api/Batch/{batchId}` - Get batch status and results

**Configuration**:
- `ServiceUrls__LookupServiceBaseUrl` - IP Lookup service URL
- Batch size: 3 IPs per chunk (configurable)

## 📊 Data Models

### IpDetails
```json
{
  "ip": "8.8.8.8",
  "type": "IPv4",
  "continentCode": "NA",
  "continentName": "North America",
  "countryCode": "US",
  "countryName": "United States",
  "regionCode": "CA",
  "regionName": "California",
  "city": "Mountain View",
  "zip": "94043",
  "latitude": 37.386,
  "longitude": -122.0838,
  "retrievedAt": "2024-01-20T10:30:00Z"
}
```

### Batch Request/Response Models
```json
// POST /api/Batch Request
{
  "ipAddresses": ["8.8.8.8", "1.1.1.1", "208.67.222.222"]
}

// POST /api/Batch Response
{
  "batchId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Running"
}

// GET /api/Batch/{batchId} Response
{
  "batchId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Completed",
  "totalIps": 3,
  "processedIps": 3,
  "results": [/* Array of IpDetails */]
}
```

## 🐳 Docker Deployment

### Prerequisites
- Docker Engine 20.10+
- Docker Compose 2.0+

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd Timspark-Tech-Task
   ```

2. **Configure API Key**:
   Edit `docker-compose.yml` and replace `your_ipstack_api_key_here` with your actual IPStack API key:
   ```yaml
   - IpStack__ApiKey=your_actual_api_key_here
   ```

3. **Build and start all services**:
   ```bash
   docker compose up --build -d
   ```

4. **Verify services are running**:
   ```bash
   docker compose ps
   ```

### Service URLs
- **BatchProcessorService**: http://localhost:5082
- **IpLookupService**: http://localhost:5101
- **CacheService**: http://localhost:5067

### Docker Commands

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# View logs for specific service
docker compose logs -f batch-processor-service

# Stop all services
docker compose down

# Rebuild and restart
docker compose up --build -d
```

## 🔧 Configuration

### Environment Variables

#### BatchProcessorService
- `ASPNETCORE_ENVIRONMENT` - Environment (Production/Development)
- `ServiceUrls__LookupServiceBaseUrl` - IP Lookup service URL

#### IpLookupService
- `ASPNETCORE_ENVIRONMENT` - Environment (Production/Development)
- `IpStack__ApiKey` - IPStack API key
- `ServiceUrls__CacheServiceBaseUrl` - Cache service URL

#### CacheService
- `ASPNETCORE_ENVIRONMENT` - Environment (Production/Development)
- `CacheSettings__CacheExpiration` - Cache expiration in minutes

## 📝 API Usage Examples

### 1. Single IP Lookup
```bash
curl -X GET "http://localhost:5101/api/IpLookup/8.8.8.8"
```

### 2. Batch Processing
```bash
# Start batch processing
curl -X POST "http://localhost:5082/api/Batch" \
  -H "Content-Type: application/json" \
  -d '{"ipAddresses": ["8.8.8.8", "1.1.1.1", "208.67.222.222"]}'

# Check batch status
curl -X GET "http://localhost:5082/api/Batch/{batchId}"
```

### 3. Cache Operations
```bash
# Get cached IP details
curl -X GET "http://localhost:5067/api/IpCache/8.8.8.8"

# Store IP details in cache
curl -X POST "http://localhost:5067/api/IpCache" \
  -H "Content-Type: application/json" \
  -d '{"ip": "8.8.8.8", "countryName": "United States", ...}'
```

## 🔄 Service Communication Flow

1. **Batch Processing Flow**:
   ```
   Client → BatchProcessorService → IpLookupService → CacheService
                                    ↓
                              External IP API (IPStack)
   ```

2. **Cache-First Strategy**:
   ```
   IpLookupService → CacheService (check cache)
                    ↓ (if not found)
                    External IP API (IPStack)
                    ↓ (store result)
                    CacheService
   ```

3. **Asynchronous Processing**:
   - Batch requests return immediately with batch ID
   - Processing happens in background using `Task.Run`
   - Status can be queried using batch ID
   - Results are stored and retrievable

## 🛡️ Error Handling

### Custom Exceptions
- `IPServiceNotAvailableException` - External IP service unavailable
- `BadRequestException` - Invalid request data
- `NotFoundException` - Resource not found

### Error Response Format
```json
{
  "status": 400,
  "message": "Invalid IP addresses in the batch request.",
  "traceId": "0HNGGCQ7F00I0:00000001"
}
```

## 📈 Performance Features

- **Asynchronous Processing**: Non-blocking batch operations
- **Chunk Processing**: Configurable batch sizes for optimal performance
- **Memory Caching**: Fast in-memory cache with TTL
- **Connection Pooling**: HTTP client reuse for external API calls
- **Logging**: Comprehensive logging with Serilog
- **Health Monitoring**: Service status tracking

## 🔍 Monitoring and Logging

- **Structured Logging**: Using Serilog with console and file outputs
- **Request Tracing**: Unique trace IDs for request tracking
- **Performance Metrics**: Processing time and success rate tracking
- **Error Monitoring**: Comprehensive error logging and handling

## 🧪 Testing

### Manual Testing
1. Start all services using Docker Compose
2. Test individual endpoints using curl or Postman
3. Verify batch processing with multiple IP addresses
4. Check cache behavior and expiration

### Integration Testing
- Services communicate through Docker network
- End-to-end testing of complete workflow
- Error scenario testing (invalid IPs, service failures)

## 🚀 Production Considerations

### Security
- Non-root container users
- Environment variable configuration
- Input validation and sanitization
- Rate limiting (recommended for production)

### Scalability
- Horizontal scaling with Docker Compose
- Load balancing capabilities
- Database integration for persistent storage
- Message queue integration for high-volume processing

### Monitoring
- Health check endpoints
- Metrics collection
- Centralized logging
- Alerting and notification systems

## 📚 Technical Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Docker** - Containerization
- **Docker Compose** - Orchestration
- **Serilog** - Logging framework
- **FluentValidation** - Input validation
- **Swagger/OpenAPI** - API documentation
- **IPStack** - External IP lookup service
- **MemoryCache** - In-memory caching

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is part of a technical interview assessment for ODIN Konsult AS.

---

**Contact**: ODIN Konsult AS – Strandveien 37 – Lysaker 1366 – Norway +47 (942) 67 372