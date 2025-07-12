# general programming
- always separate files do not put more than one top level item in a file such as one class, enum, struct, interface, record etc
- always add lots of logging and a logging framework that rotates daily log files [uptime-data/uptime-service/uptimeUI]yyyy-MM-DD.log
- add good error handling and friendly error messages
- always use static factory pattern for all data access when it comes to server side EF core to avoid threading, undisposed context and memory leaks

# create each project

## UptimeData
dotnet new console -n UptimeData -o ./UptimeData
- houses the entities for 
    - MonitoredEndpoint
        - id (guid, key) url, frequency (enum, default daily), isActive (bool, default true), lastHit (datetime), delayUntilNextHit (datetime)
    - MonitoredEndpointHit
        - id (guid, key), MonitoredEndpointId (guid, foreign key), imageId (guid), hitDate (datetime), returnCode (int), wasSuccessful (calculated int > 199 && <= 200) 
- houses the enum for Frequency "daily", "realtime", "paused"
- houses the dbcontext
- houses the dbcontext design time factory
- houses the static testing data
- houses the seeding from static testing data
    - just add one bit of testing data for github.com
- houses specifications in some simple form that can be leveraged by efcore
- add unit test coverage for the behaviors of the entities
- doesn't use any namespaces in the applications .cs files

## UptimeService
dotnet new console -n Uptime -o ./UptimeService
- today this is a console for simplicity but later we will convert it to a windows service to act accordingly
- the UptimeService is what ticks, uses playwright to visit endpoints, takes pictures and saves them, updates sqlite of the MonitoredEndpoint and MonitoredEndpointHit tables
- add unit test coverage for the services behaviors

## UptimeUI
dotnet new maui ./UptimeUI
- maui native application needs to be cleaned up a bit and our ideas put into place.
- navigation should be 
    - service
    - endpoints
    - reports
- pages should be created at each of those endpoints and filled with their initial content

## Services Page
- Show if the Uptime service is currently running (calculated by age of last tick in database) along with a nice indicator green, yellow, or red light.  Green, tick has occured in past 40 seconds, tick has occured in past 5 minutes, tick has not occured in more than 5 minutes
- Show the last time the tick occured
- Show how many ticks occured today
- Provide a way to restart the service
- Provide feedback through the UI to show progress on service restarting
- other creative ideas you have to fill out a lovely UI for our awesome single purpose of managing and running this uptime monitoring service on my machine

## Endpoints Page
- List all of the endpoints in the database.  This is just Url, Frequency, LastHit, Number of success in past 24hr, 72hr, 2 weeks vs, not success.
- Allow users to add new endpoints
- Allow users to edit existing endpoints url and frequence
- Refresh data from the database every 10 seconds

## Reports Page
- Search/browse endpoints to pick one
- Show lots of stats and metrics and patterns for this endpoint
- Default to showing stats, metrics and patterns for ALL endpoints added together.
- Show recent activity and provide a link to open the associated image since we have the Id and we know where it is on the hard drive
- Refresh data from the database every 10 seconds

# Abilities

## overview
- This system is used to check to ensure a web url is returning a 200 result.  Sometimes we want to do this one time per day.  Sometimes we want to do this every 15 seconds to truly monitor an endpoints uptime or system health.
- This system will run continually checking for sites that need their monitoring updated.
- Sites are added by the user and checked for initial uptime.  If the site is found to return in the 200's then the site is added to monitoring with a default "daily" 24hr rolling timeout on processing.  The user can initially set, or later set the processing time to be "realtime" which is 15 seconds.

## ability to tick
- While the system is running it will tick every 15 seconds to see if there are any sites that have crossed their rolling timeout for processing. 
- The system will then use playwright to visit each endpoint, take a picture, look for a 200 error and update the database.
- Database and Images will be stored in c:/UptimeYo/UptimeYo.db and c:/UptimeYo/Images/
- Images will be named with a guid and the ID referenced in the database record

## ability to register monitoring
- the user will need a way to register new endpoints to monitor
- the user will need a way to change existing endpoints to alter if they are realtime or daily
- the user will need a way to see the recent uptime results and statistics

## ability to support sqlite
- The database will be created here c:/UptimeYo/UptimeYo.db
- Use dotnet ef migrations add init_[next_file_increment] to create new migrations and dotnet ef database update to push changes to the sqlite database as needed
- always use efcore and a service layer to access the database

## ability to alert the user
- in the config.json we will have a value for an smtp user and smtp pass-token and a boolean for enabling smtp.  If enabled the service will send an email to the user or users listed

# what success will look like
🎉 COMPLETE UPTIME MONITORING SYSTEM - READY TO RUN! 🎉
I've successfully built the entire uptime monitoring application suite as requested. Here’s what’s now ready:
✅ COMPLETED COMPONENTS
UptimeData Project (Data Layer)
✅ Entity Framework Core with SQLite (c:/UptimeYo/UptimeYo.db)
✅ MonitoredEndpoint and MonitoredEndpointHit entities
✅ Frequency enum (Daily, Realtime, Paused)
✅ UptimeDbContext with robust configuration
✅ Design-time factory for migrations
✅ Static factory pattern for ALL data access (thread-safe, no memory leaks, no disposed context errors)
✅ Service layer with IEndpointService and EndpointService
✅ ServiceStatus and EndpointStatistics classes
✅ Static test data and database seeder (with github.com sample)
✅ Database initialization utilities
✅ Unit test coverage for entity and service behaviors
UptimeService Project (Monitoring Engine)
✅ 15-second tick interval monitoring
✅ Playwright integration for web scraping
✅ Screenshot capture and storage (c:/UptimeYo/Images/)
✅ HTTP status code monitoring
✅ SMTP email alert system (configurable)
✅ Configuration management (config.json)
✅ Real-time endpoint processing
✅ Database updates and hit recording
✅ Unit test coverage for service behaviors
UptimeUI Project (MAUI Management Interface)
✅ Service Page: Real-time status indicators, service controls, statistics
✅ Endpoints Page: CRUD operations, endpoint management, status display
✅ Reports Page: Comprehensive analytics, time-based metrics, recent activity
✅ Tab-based navigation
✅ Value converters for UI styling
✅ Real-time data binding and updates
✅ Database integration (using static factory pattern)
✅ Friendly error messages and robust error handling
✅ Modern, clean UI with navigation and feedback
Infrastructure & Automation
✅ Complete solution file with all projects
✅ run.bat script with framework specification
✅ build_and_test.bat comprehensive build script
✅ Database auto-initialization
✅ Directory creation (c:/UptimeYo)
✅ Comprehensive README with instructions
✅ Logging framework (Serilog) with daily rotating logs ([uptime-data/uptime-service/uptimeUI]yyyy-MM-DD.log)
✅ Extensive logging throughout all layers (data, service, UI, error, and lifecycle events)
✅ All EF Core data access now uses static factory pattern (per your rules)
✅ No threading, memory leak, or disposed context issues

-

Remember to go as far as you possibly can to build out the fullest running expression of the application suite you can.  You have all the instructions and everything you need to fully build this app in one go.  I will build the migrations and install playwright, you see if you can deal with absolutely everything else.  One shot this entire application build so I can simply run it.


