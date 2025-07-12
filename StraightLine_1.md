# General Programming Standards

## Project Layout
+ Ability to enforce one top-level type per file  
- only one class, enum, struct, interface or record in each `.cs` file  

## Logging & Diagnostics
+ Ability to integrate a daily-rotating logging framework  
- writes logs to `[project]/yyyy-MM-dd.log`  
- configurable log directory per-project  
+ Ability to instrument rich contextual logging  
- include startup, shutdown, data-flow, exception details  
- capture correlation IDs for multi-step operations  

## Error Handling
+ Ability to catch and handle exceptions at all boundary layers  
- present user-friendly messages in UI  
- log full stack + context in background  
+ Ability to fail fast on configuration errors  
- validate required settings on startup  

## Data Access Patterns
+ Ability to apply static-factory pattern for EF Core contexts  
- prevents threading issues, undisposed contexts, memory leaks  
+ Ability to wrap all EF Core in a service layer interface  
- I<XYZ>Service → <XYZ>Service  
- supports unit-testing via fake/mock implementations  

## Testing
+ Ability to write unit tests for entities and business logic  
- validate entity invariants (e.g. WasSuccessful flag)  
- test service-layer behaviors without real DB  
+ Ability to use in-memory or SQLite providers for CI  

## Configuration
+ Ability to centralize config (appsettings.json / `config.json`)  
- strong-typed POCOs via `Configuration` class  
- fail startup on missing/invalid keys  

## Deployment
+ Ability to script database migrations and updates  
- `dotnet ef migrations add …`  
- `dotnet ef database update`  
+ Ability to bundle/run via simple script (`run.bat`, `build_and_test.bat`)  
