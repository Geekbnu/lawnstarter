# About the Solution

Solution developed with .NET 8 backend using MongoDB and React frontend. For metrics, I used VictoriaMetrics+Influx. Docker is used to run the application.

## Technical Requirements
To run the application, ensure you have **Docker** installed on your computer.

## Running the Application

Navigate to the src folder in your terminal and execute the following command:
```
docker-compose up -d
```
After execution, the following will be available:
- [Frontend](http://localhost:8080)
- [Backend](http://localhost:9090/api/movies/1)

## Backend Endpoints

The backend has two basic routes:
- [Movies -> http://localhost:9090/api/movies/1](http://localhost:9090/api/movies/1) (Example)
- [People -> http://localhost:9090/api/people/1](http://localhost:9090/api/people/1) (Example)



The metrics platform also provides a [graphical portal](http://localhost:8428/vmui) for detailed queries of all metrics. Over 300 metrics are collected and can be queried through the [endpoint](http://localhost:9090/metrics).



## Architecture & Design

### Technology Stack
- **Backend**: .NET 8 with ASP.NET Core
- **Database**: MongoDB
- **Frontend**: React
- **Metrics**: VictoriaMetrics + InfluxDB
- **Containerization**: Docker

### System Architecture
The application follows a microservices architecture with clear separation between frontend and backend components, enhanced with comprehensive metrics collection.

## Development Setup

### Prerequisites
- Docker and Docker Compose
- .NET 8 SDK (for local development)
- Node.js and npm (for frontend development)

### Local Development
```bash
# Clone the repository
git clone [repository-url]

# Navigate to source directory
cd src

# Start all services
docker-compose up -d

# For development with hot reload
docker-compose -f docker-compose.dev.yml up
```

### Environment Variables
Create a `.env` file in the src directory with the following variables:
```
MONGODB_CONNECTION_STRING=mongodb://localhost:27017
DATABASE_NAME=your_database_name
METRICS_ENDPOINT=http://localhost:8428
```

## API Documentation

### Authentication
Currently, the API does not require authentication. For production use, consider implementing JWT or OAuth2.

### Response Format
All API responses follow this structure:
```json
{
  "success": true,
  "data": {},
  "message": "Success",
  "timestamp": "2025-06-03T10:00:00Z"
}
```

### Error Handling
Error responses include appropriate HTTP status codes and descriptive messages:
```json
{
  "success": false,
  "error": "Resource not found",
  "statusCode": 404,
  "timestamp": "2025-06-03T10:00:00Z"
}
```

## Testing

### Running Tests
```bash
# Run backend tests
dotnet test
```

### Test Coverage
- Unit tests cover business logic 

## Monitoring & Observability

### Metrics

The application presents some metrics for this exam:
- [Top searches performed](http://localhost:9090/api/movies/stats/top-searches)
- [Performance statistics](http://localhost:9090/api/movies/stats/performance)
- [Statistics by time](http://localhost:9090/api/movies/stats/hourly)
- [Top Searches](http://localhost:9090/api/movies/stats/top-searches)
- [Status codes distribution](http://localhost:9090/api/movies/stats/status-codes)

The application will reset the metrics every 5 minutes.

> [!IMPORTANT]
> Only the movies route is being monitored by metrics.

### Health Checks
- [Application Health](http://localhost:9090/api/people/health)

### Logging
Logs are structured in JSON format and can be accessed via:
```bash
docker-compose logs -f [service-name]
```

## Deployment

### Production Deployment
1. Set environment-specific variables
2. Use production Docker Compose file
3. Configure reverse proxy (nginx recommended)
4. Set up SSL certificates
5. Configure monitoring alerts

### Scaling Considerations
- Backend services can be horizontally scaled
- MongoDB supports replica sets for high availability
- Consider implementing caching layer for high-traffic scenarios

## Performance

### Expected Performance
- Average response time: < 200ms
- Throughput: 1000+ requests/second
- Database queries: < 50ms average

### Optimization Tips
- Enable MongoDB indexing for frequently queried fields
- Implement response caching for static data
- Use connection pooling for database connections

## Security

### Security Measures
- Input validation and sanitization
- CORS configuration for cross-origin requests
- Rate limiting to prevent abuse
- Secure headers implementation

### Recommendations for Production
- Implement authentication and authorization
- Use HTTPS for all communications
- Regular security audits and dependency updates
- Environment variable encryption

## Troubleshooting

### Common Issues

**Port Conflicts**
```bash
# Check port usage
netstat -tulpn | grep :8080

# Change ports in docker-compose.yml if needed
```

**Database Connection Issues**
```bash
# Check MongoDB container status
docker-compose ps

# View MongoDB logs
docker-compose logs mongodb
```

**Metrics Not Collecting**
- Verify VictoriaMetrics container is running
- Check network connectivity between services
- Validate metrics endpoint configuration


