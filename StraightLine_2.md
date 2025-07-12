# Uptime Monitoring Build

## 1. Data Layer (UptimeData)

### 1.1 Entities
+ Ability to model `MonitoredEndpoint`  
- Id: Guid (PK), Url: string, Frequency: enum {Daily,Realtime,Paused}, IsActive: bool, LastHit: DateTime, DelayUntilNextHit: DateTime  
+ Ability to model `MonitoredEndpointHit`  
- Id: Guid (PK), MonitoredEndpointId: Guid (FK), ImageId: Guid, HitDate: DateTime, ReturnCode: int, WasSuccessful: bool (>199 && <=299)  

### 1.2 Enumeration
+ Ability to define `Frequency` enum  
- values: Daily, Realtime, Paused  

### 1.3 DbContext & Factory
+ Ability to configure `UptimeDbContext`  
- DbSets for endpoints & hits  
+ Ability to provide design-time `UptimeDbContextFactory`  

### 1.4 Seeding & Migrations
+ Ability to seed static test data (GitHub.com example) in `DatabaseSeeder`  
+ Ability to apply EF Core migrations (`init_X`) and update SQLite  

### 1.5 Service Layer
+ Ability to expose `IEndpointService` → `EndpointService`  
- CRUD for endpoints  
- statistical queries (counts, rates)  

### 1.6 Testing
+ Ability to cover entity invariants and `EndpointService` logic with unit tests  

## 2. Monitoring Engine (UptimeService)

### 2.1 Core Scheduler
+ Ability to tick on a 15-second interval  
- detects endpoints past their rolling timeout (Daily vs Realtime)  

### 2.2 Playwright Integration
+ Ability to launch Playwright browser per tick  
- visit URL, capture screenshot, extract HTTP status  

### 2.3 Persistence
+ Ability to write hits to SQLite at `C:/UptimeYo/UptimeYo.db`  
+ Ability to save screenshots to `C:/UptimeYo/Images/{Guid}.png`  

### 2.4 Alerts
+ Ability to configure SMTP in `config.json`  
- fields: SmtpUser, SmtpPass, Enabled, RecipientList  
+ Ability to send email on failure via `SmtpAlertService`  

### 2.5 Configuration & Startup
+ Ability to load and validate settings in `Configuration` class  
+ Ability to fail fast if DB or SMTP config invalid  

### 2.6 Future Extension
+ Ability to convert console to Windows Service with same tick logic  

## 3. Management UI (UptimeUI – .NET MAUI)

### 3.1 App Shell & Navigation
+ Ability to scaffold MAUI Shell (`AppShell`)  
- Tabs: Service, Endpoints, Reports  

### 3.2 Service Page
#### 3.2.1 Status Display
+ Ability to show status indicator (green/yellow/red) based on last tick age  
- Green: <40s, Yellow: <5m, Red: ≥5m  
+ Ability to show Last Tick timestamp + “3 seconds ago” style label  
+ Ability to show “Ticks Today” count  

#### 3.2.2 Controls
+ Ability to Restart Service (button) with UI feedback  
+ Ability to Refresh Status (button)  

#### 3.2.3 Quick Stats
+ Ability to show total vs active endpoints, success rate, failures today  

### 3.3 Endpoints Page
#### 3.3.1 List View
+ Ability to list all endpoints with:
  - URL, last hit timestamp, status indicator  
  - Frequency badge (Daily/Realtime)  
#### 3.3.2 CRUD Actions
+ Ability to Add Endpoint (modal/page)  
+ Ability to Edit Endpoint (URL & Frequency)  
+ Ability to Delete Endpoint  
#### 3.3.3 Auto-refresh
+ Ability to reload list every 10 seconds  

### 3.4 Reports Page
#### 3.4.1 Endpoint Selector
+ Ability to choose single endpoint or “All Endpoints” via picker  
+ Ability to “Show Report” button  

#### 3.4.2 Overall Stats
+ Ability to display Total Hits, Successful Hits, Failed Hits, Success Rate, Last Success/Failure  

#### 3.4.3 Time-based Stats
+ Ability to show summaries for 24h, 72h, 2w, plus average response time  

#### 3.4.4 Recent Activity Chart
+ Ability to render a timeline of recent hits with status indicators (green/red)  

### 3.5 Common UI Concerns
+ Ability to bind data in real time via MVVM  
+ Ability to implement value converters (status → color)  
+ Ability to catch and display friendly errors  
+ Ability to theme with clean, minimal XAML styles (`Colors.xaml`, `Styles.xaml`)  
