# 📊 Uptime Monitor

A comprehensive uptime monitoring system built with .NET 8, featuring real-time monitoring, web-based dashboard, and intelligent alerting.

## 🏗️ Architecture

The system consists of three main components:

- **UptimeData**: Entity Framework Core data layer with SQLite database
- **UptimeService**: Background monitoring service with Playwright integration
- **UptimeUI**: Blazor Server web application for management and reporting

## ✨ Features

### 📈 Service Status Dashboard
- Real-time service health monitoring
- Visual status indicators (Green/Yellow/Red)
- Quick statistics and metrics
- Service control buttons

### 🌐 Endpoint Management
- Add, edit, and delete monitored endpoints
- Support for different monitoring frequencies (Realtime, Daily, Paused)
- Auto-refresh every 10 seconds
- URL validation and status tracking

### 📊 Reports & Analytics
- Comprehensive statistics dashboard
- Time-based reporting (24h, 72h, 2 weeks)
- Success rate calculations
- Recent activity timeline
- Endpoint-specific or aggregate reporting

### 🔍 Monitoring Engine
- Playwright-based web scraping
- Screenshot capture for each check
- HTTP status code tracking
- Configurable monitoring intervals
- Intelligent retry logic

### 📧 Alert System
- SMTP-based email notifications
- Configurable recipient lists
- Rich HTML and text email templates
- Failure threshold management

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Linux/macOS/Windows environment

### Installation & Setup

1. **Clone and build the project:**
   ```bash
   ./build_and_test.sh
   ```

2. **Start the monitoring service:**
   ```bash
   ./run_service.sh
   ```

3. **Start the web UI (in a new terminal):**
   ```bash
   ./run_ui.sh
   ```

4. **Access the dashboard:**
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001

## 📁 Project Structure

```
├── src/
│   ├── UptimeData/           # Data layer & EF Core models
│   │   ├── MonitoredEndpoint.cs
│   │   ├── MonitoredEndpointHit.cs
│   │   ├── UptimeDbContext.cs
│   │   ├── EndpointService.cs
│   │   └── DatabaseSeeder.cs
│   ├── UptimeService/        # Background monitoring service
│   │   ├── MonitoringService.cs
│   │   ├── SmtpAlertService.cs
│   │   ├── Configuration.cs
│   │   └── config.json
│   └── UptimeUI/            # Blazor web application
│       ├── Pages/
│       │   ├── Home.razor          # Service status dashboard
│       │   ├── Endpoints.razor     # Endpoint management
│       │   └── Reports.razor       # Analytics & reporting
│       └── Components/
├── tests/
│   └── UptimeData.Tests/    # Unit tests
└── Scripts/
    ├── build_and_test.sh
    ├── run_service.sh
    └── run_ui.sh
```

## ⚙️ Configuration

### Monitoring Service Configuration

Edit `src/UptimeService/config.json`:

```json
{
  "DatabasePath": "/tmp/UptimeYo/UptimeYo.db",
  "ImagesPath": "/tmp/UptimeYo/Images",
  "LogDirectory": "/tmp/UptimeYo/Logs",
  "MonitoringIntervalSeconds": 15,
  "Smtp": {
    "Enabled": false,
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "SmtpPass": "your-app-password",
    "UseSSL": true,
    "RecipientList": ["admin@example.com"],
    "FromAddress": "uptime-monitor@example.com",
    "FromName": "Uptime Monitor"
  }
}
```

### Email Alerts Setup

1. Enable SMTP in the configuration
2. Configure your email provider settings
3. Add recipient email addresses
4. Test with a failing endpoint

## 🗄️ Database Schema

### MonitoredEndpoint
- `Id` (Guid): Primary key
- `Url` (string): Endpoint URL to monitor
- `Frequency` (enum): Monitoring frequency (Daily, Realtime, Paused)
- `IsActive` (bool): Whether endpoint is active
- `LastHit` (DateTime): Last monitoring timestamp
- `DelayUntilNextHit` (DateTime): Next scheduled check

### MonitoredEndpointHit
- `Id` (Guid): Primary key
- `MonitoredEndpointId` (Guid): Foreign key to endpoint
- `ImageId` (Guid): Screenshot identifier
- `HitDate` (DateTime): Hit timestamp
- `ReturnCode` (int): HTTP status code
- `WasSuccessful` (computed): Success indicator (200-299 range)

## 📋 API Endpoints

The system provides the following service methods:

### Endpoint Management
- `GetAllEndpointsAsync()`: Retrieve all endpoints
- `GetActiveEndpointsAsync()`: Retrieve active endpoints only
- `CreateEndpointAsync()`: Create new endpoint
- `UpdateEndpointAsync()`: Update existing endpoint
- `DeleteEndpointAsync()`: Delete endpoint

### Hit Tracking
- `CreateHitAsync()`: Record monitoring hit
- `GetHitsByEndpointIdAsync()`: Retrieve hits for endpoint
- `GetRecentHitsAsync()`: Get recent monitoring activity

### Statistics
- `GetSuccessRateAsync()`: Calculate overall success rate
- `GetStatsForPeriodAsync()`: Get statistics for date range
- `GetStatsLast24HoursAsync()`: Get 24-hour statistics
- `GetStatsLast72HoursAsync()`: Get 72-hour statistics
- `GetStatsLast2WeeksAsync()`: Get 2-week statistics

## 🧪 Testing

Run the complete test suite:

```bash
dotnet test
```

The test suite includes:
- Entity validation tests
- Service layer unit tests
- Database integration tests
- Business logic verification
- Success rate calculations
- CRUD operations testing

## 📊 Sample Data

The system automatically seeds with sample endpoints:
- GitHub (https://github.com) - Realtime monitoring
- Google (https://google.com) - Daily monitoring
- Microsoft (https://microsoft.com) - Realtime monitoring
- Stack Overflow (https://stackoverflow.com) - Paused

## 🔒 Security Considerations

- Database stored in local file system
- HTTPS support for web UI
- Email credentials stored in configuration
- Screenshot storage in local directory
- No authentication implemented (add as needed)

## 🛠️ Development

### Adding New Features

1. **New Monitoring Metrics**: Extend `MonitoredEndpointHit` entity
2. **Additional Alerts**: Implement new alert service interfaces
3. **Custom Frequencies**: Add to `Frequency` enum
4. **New Reports**: Add pages to `UptimeUI/Pages/`

### Debugging

- Logs are written to `{LogDirectory}/yyyy-MM-dd.log`
- Screenshots saved to `{ImagesPath}/{Guid}.png`
- Database can be inspected with SQLite tools

## 📈 Performance

- Monitoring interval: 15 seconds (configurable)
- Playwright browser reuse for efficiency
- Database connection pooling
- Async/await throughout
- In-memory caching for statistics

## 🔄 Deployment

### Production Deployment
1. Update configuration paths for production
2. Configure proper SMTP settings
3. Set up reverse proxy (nginx/Apache)
4. Configure systemd service files
5. Set up log rotation
6. Configure backup strategy for SQLite database

### Docker Deployment
```dockerfile
# Example Dockerfile structure
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
RUN apt-get update && apt-get install -y chromium-browser
EXPOSE 5000
ENTRYPOINT ["dotnet", "UptimeService.dll"]
```

## 📚 Technologies Used

- **.NET 8**: Core framework
- **Entity Framework Core**: ORM with SQLite
- **Blazor Server**: Web UI framework
- **Playwright**: Web automation and screenshots
- **Bootstrap 5**: UI styling
- **MailKit**: Email sending
- **NLog**: Logging framework
- **xUnit**: Unit testing
- **Moq**: Mocking framework

## 🤝 Contributing

1. Fork the repository
2. Create feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit pull request

## 📜 License

This project is licensed under the MIT License.

## 🆘 Support

For issues and questions:
1. Check existing issues in the repository
2. Review configuration settings
3. Check log files for errors
4. Verify .NET 8 SDK installation

---

**Built with ❤️ using .NET 8 and modern web technologies**