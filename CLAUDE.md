# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Glutspeicher is a password management system with a server-client architecture. The server is an ASP.NET Core 9.0 web application that provides a REST API and web frontend, while the client is a Windows application that handles automation and relay functionality.

## Build & Run Commands

### Server

```bash
# Build the server
dotnet build "Glutspeicher Server/Glutspeicher Server.csproj"

# Run the server in development mode
dotnet run --project "Glutspeicher Server/Glutspeicher Server.csproj"

# Build for release (Windows)
dotnet publish "Glutspeicher Server/Glutspeicher Server.csproj" -c Release -r win-x64 --self-contained -o "Glutspeicher Server/Build/Windows"

# Build for release (Linux)
dotnet publish "Glutspeicher Server/Glutspeicher Server.csproj" -c Release -r linux-x64 --self-contained -o "Glutspeicher Server/Build/Linux"
```

### Client

```bash
# Build the client
dotnet build "Glutspeicher Client/Glutspeicher Client.csproj"

# Run the client
dotnet run --project "Glutspeicher Client/Glutspeicher Client.csproj"
```

### Docker

```bash
# Build and run using Docker Compose
docker-compose up

# Build from source
docker-compose -f docker-compose-from-source.yml up
```

## Environment Configuration

### Required Environment Variables

- `GLUTSPEICHER_CRYPTO_KEY`: A 32-character encryption key used for database encryption
  - In DEBUG mode: Automatically set to "00000000000000000000000000000000"
  - In RELEASE mode (Windows): Automatically generated if not set, stored as machine-level environment variable
  - For Docker: Set in docker-compose.yml (defaults to test key)

### Configuration Files

- `Glutspeicher Server/appsettings.json`: Contains HttpPort, ServerVersion, and ClientVersion settings
  - HttpPort: Default 80 (on Windows, adds 5500 to become 5580)
  - ServerVersion and ClientVersion must match between builds

## Architecture

### Server Architecture

The server follows a modular ASP.NET Core architecture:

**Entry Point** (`Program.cs`):
- Loads configuration from appsettings.json
- In RELEASE mode on Windows: Handles Windows Service registration and admin elevation
- Sets up dependency injection with LiteDbContext
- Configures middleware pipeline: API routing → Frontend → NoCache

**Database Layer** (`Context/LiteDbContext.cs`):
- Uses LiteDB for embedded NoSQL storage
- Encrypts database files at rest using AES encryption with GLUTSPEICHER_CRYPTO_KEY
- Stores encrypted database in `Data/Database.litedb.encrypted`
- Creates timestamped backups (compressed and encrypted) on every write
- Thread-safe with SemaphoreSlim locking
- Loads database into memory on startup for performance

**API Layer** (`Mapping/Api.*.cs`):
- Organized by resource type: Passwords, Generators, Exports, Relays
- Each resource follows CRUD pattern: GetAll, Get, Post, Put, Delete
- Additional Export endpoint returns CSV files
- Uses custom ApiResult response wrapper for consistent API responses
- All routes mounted via `Routing/ExtensionMethods.UseApi()`

**Frontend Middleware** (`Middleware/FrontendMiddleware.cs`):
- Serves static files from `wwwroot/` directory
- Implements server-side include system for HTML, CSS, and JS files:
  - HTML: `<!-- include path/to/file.html -->`
  - CSS: `/* include path/to/file.css */`
  - JS: `// include path/to/file.js`
- Compiles SCSS to CSS on-the-fly using LibSassHost
- Injects SERVER_VERSION, CLIENT_VERSION, and DEBUG flags into JS files
- Maps `/` to `/index.html` and handles CSV exports from `/static/exports/`

**Models** (`Model/`):
- Password: Core entity with static/generated passwords, TOTP support, relay connections
- Generator: Password generation rules
- Export: CSV export configurations
- Relay: Client relay connection definitions

### Frontend Structure

The frontend is a vanilla JavaScript SPA located in `wwwroot/static/`:
- `js/`: Core JavaScript modules (app.js, data.js, component.js, page.js, extensions.js)
- `css/`: SCSS stylesheets with component-based organization
- `page/`: Individual page modules (passwords, generators, exports, relays, settings)
- Uses server-side includes to compose pages from reusable components
- SCSS files are compiled on-demand by the server

### Client Architecture

Located in `Glutspeicher Client/`:
- Windows-only application for automation and relay functionality
- Handles AutoType functionality (keyboard automation)
- Manages relay sessions for remote connections
- Interacts with the server API

## Key Technical Details

### Encryption & Security

- All database files are encrypted at rest using AES-256
- Encryption key must be exactly 32 bytes/characters
- IV is randomly generated per encryption operation and prepended to encrypted data
- Database is decrypted into memory on startup for performance

### API Patterns

- All API endpoints return IApiResult with consistent structure:
  ```json
  {
    "success": bool,
    "errorMessage": string | null,
    "listInfo": { /* pagination/filtering info */ } | null,
    "data": object | array | null
  }
  ```
- Use `SetDirty()` on LiteDbContext after any write operation to trigger backup on disposal

### Build System

- Targets .NET 9.0
- Uses LibSassHost for SCSS compilation (native binaries for Windows and Linux)
- Windows builds can run as Windows Service
- Build output goes to `Build/Windows/` or `Build/Linux/`
- wwwroot content is copied to build directories

## Development Notes

- The server port automatically shifts from 80 to 5580 on Windows during development
- Database backups are automatically created with timestamps in `Data/` directory
- Frontend includes are resolved at request time, so changes are immediately visible
- SCSS compilation happens on every request in both DEBUG and RELEASE modes
- Use `SetDirty()` on LiteDbContext after modifications to ensure persistence
