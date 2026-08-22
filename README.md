# Glutspeicher

**A secure, self-hosted password management system with advanced automation and relay capabilities.**

Glutspeicher is a modern password manager built with a server-client architecture, designed for users who value security, privacy, and control over their credentials. With enterprise-grade encryption, automatic backup functionality, and powerful automation features, Glutspeicher provides a comprehensive solution for managing passwords across local and remote systems.

## Key Features

### Secure Password Storage
- **AES-256 Encryption**: All passwords are stored in an encrypted LiteDB database with AES encryption at rest
- **Automatic Backups**: Every database modification creates a timestamped, compressed, and encrypted backup
- **Self-Hosted**: Complete control over your data - no third-party services or cloud dependencies
- **Memory-Resident Database**: Database is loaded into memory on startup for optimal performance

### Password Management
- **Static & Generated Passwords**: Support for both manually entered passwords and rule-based password generation
- **Password Generators**: Define custom password generation rules with configurable length and character sets
- **TOTP Support**: Built-in Time-based One-Time Password (TOTP) support for two-factor authentication
- **CSV Export**: Export password collections to CSV format for backup or migration
- **Organized Storage**: Group passwords by sections and sources for easy navigation

### Advanced Connectivity
- **Protocol Support**: Native support for HTTP/HTTPS, SSH, and RDP connections
- **Relay Sessions**: Connect to remote systems through SSH relay servers for secure access to isolated networks
- **AutoType Functionality**: Automated keyboard input for username/password entry (Windows Client)
- **GlutLink Protocol**: Custom URI scheme for deep integration between server and client applications

### Modern Web Interface
- **Single Page Application**: Fast, responsive vanilla JavaScript frontend
- **Real-time SCSS Compilation**: Server-side SCSS preprocessing for modern styling
- **Component-Based Architecture**: Modular design with server-side includes for HTML, CSS, and JS
- **Multiple Pages**: Dedicated interfaces for Passwords, Exports, Generators, Relays, and Settings

### Flexible Deployment
- **Docker Support**: Pre-built Docker images available on Docker Hub
- **Cross-Platform Server**: Runs on Windows and Linux with .NET 9.0
- **Windows Service Integration**: Automatic Windows Service registration in production mode
- **Multiple Deployment Options**: Standalone binaries, Docker containers, or from source

## Architecture

### Server Component
Built with **ASP.NET Core 9.0**, the server provides:
- RESTful API for all CRUD operations (Passwords, Generators, Exports, Relays)
- Embedded web frontend with server-side includes and SCSS compilation
- LiteDB NoSQL database with encryption and automatic backup
- Thread-safe database operations with semaphore locking
- Custom middleware pipeline for API routing, frontend serving, and cache control

**Technology Stack:**
- .NET 9.0 (ASP.NET Core)
- LiteDB 5.0.21 (embedded NoSQL database)
- LibSassHost 1.5.0 (SCSS compilation)
- CsvCSharp 0.1.6 (CSV export functionality)

### Client Component (Windows)
Built with **.NET 9.0 Windows**, the client provides:
- **AutoType**: Automated keyboard input for password entry
- **Relay Management**: SSH tunnel creation for remote system access
- **Protocol Handler**: Custom `glut://` URI scheme support for deep integration
- **Windows Native Integration**: DPI awareness and native window management

**Technology Stack:**
- .NET 9.0 Windows
- SSH.NET 2024.2.0 (SSH connectivity)
- Newtonsoft.Json 13.0.3 (JSON processing)

### Frontend
A modern **vanilla JavaScript SPA** featuring:
- Component-based architecture with reusable UI elements
- Server-side includes for modular HTML/CSS/JS composition
- SCSS stylesheets with on-demand compilation
- Third-party libraries: jsOTP (TOTP), PapaParse (CSV), Showdown (Markdown), password-generator.js

## Getting Started

### Option 1: Docker (Recommended)

The fastest way to get started is using Docker:

```bash
# Using pre-built image from Docker Hub
docker-compose up
```

**docker-compose.yml:**
```yaml
services:
  app:
    image: melektaus/glutspeicher
    ports:
      - 8100:80
    volumes:
      - /etc/localtime:/etc/localtime:ro
      - ./data:/app/Data
    environment:
      - GLUTSPEICHER_CRYPTO_KEY=00000000000000000000000000000000
```

Access the application at `http://localhost:8100`

### Option 2: Windows Standalone

1. Download the server from the web interface or build from source
2. Run the executable - it will automatically:
   - Request administrator privileges
   - Generate a secure 32-character encryption key
   - Register as a Windows Service
   - Start the service and open your browser

### Option 3: Linux Standalone

1. Build or download the Linux server binary
2. Set the encryption key environment variable:
   ```bash
   export GLUTSPEICHER_CRYPTO_KEY="your-32-character-encryption-key"
   ```
3. Run the server:
   ```bash
   ./Glutspeicher\ Server
   ```

### Option 4: Build from Source

**Prerequisites:**
- .NET 9.0 SDK

**Build Server:**
```bash
# Development
dotnet run --project "Glutspeicher Server/Glutspeicher Server.csproj"

# Production (Windows)
dotnet publish "Glutspeicher Server/Glutspeicher Server.csproj" -c Release -r win-x64 --self-contained -o "Glutspeicher Server/Build/Windows"

# Production (Linux)
dotnet publish "Glutspeicher Server/Glutspeicher Server.csproj" -c Release -r linux-x64 --self-contained -o "Glutspeicher Server/Build/Linux"
```

**Build Client (Windows only):**
```bash
dotnet build "Glutspeicher Client/Glutspeicher Client.csproj"
```

## Configuration

### Environment Variables

**GLUTSPEICHER_CRYPTO_KEY** (Required in production)
- 32-character encryption key for database encryption
- **Debug mode**: Automatically set to `00000000000000000000000000000000`
- **Release mode (Windows)**: Auto-generated and stored as machine-level environment variable if not set
- **Docker/Linux**: Must be explicitly configured

### appsettings.json

Located in `Glutspeicher Server/appsettings.json`:

```json
{
    "HttpPort": 80,
    "ServerVersion": "0.5.9",
    "ClientVersion": "0.3.6"
}
```

- **HttpPort**: Server listening port (default: 80, automatically becomes 5580 on Windows during development)
- **ServerVersion**: Server application version (must match build version)
- **ClientVersion**: Required client version for compatibility

## Security Features

### Encryption
- **Database Encryption**: AES-256 encryption with randomly generated IV per operation
- **Encrypted Backups**: All backup files are compressed (GZip) and encrypted
- **Secure Key Storage**: Encryption key stored as machine-level environment variable (Windows) or externally managed (Docker/Linux)

### Data Protection
- **Automatic Backups**: Timestamped backups created on every database modification
- **Backup Location**: `Data/Database YYYY-MM-DD HH-mm-ss.litedb.gz.encrypted`
- **Current Database**: `Data/Database.litedb.encrypted`
- **No Cloud Storage**: All data remains on your infrastructure

### Best Practices
- Use a strong, randomly generated 32-character encryption key
- Store the encryption key securely and separately from the database
- Regularly backup the `Data` directory to external storage
- Use HTTPS reverse proxy (nginx, Caddy) for production deployments
- Limit network access to trusted clients only

## Advanced Features

### Password Generators
Define reusable password generation rules:
- Configurable length
- Custom character sets via question/answer pairs
- Generate passwords on-demand or store generated values

### Export Configurations
Create custom CSV export templates:
- Define export name and URI
- Custom JavaScript for data transformation
- Download formatted CSV files directly from the interface

### Relay System
Access systems behind firewalls or in isolated networks:
- SSH tunnel creation through relay servers
- Port range management (default: 13300-13309)
- Support for RDP, SSH, and Web protocols
- Automatic connection string generation

### AutoType (Windows Client)
Automated credential entry:
- System-wide keyboard automation
- Triggered via `glut://` protocol links
- Supports username + password sequences
- Context-aware window targeting

## API Documentation

The server exposes a RESTful API with consistent response format:

```json
{
  "success": true,
  "errorMessage": null,
  "listInfo": null,
  "data": { /* resource data */ }
}
```

### Endpoints

**Passwords**
- `GET /api/passwords` - List all passwords
- `GET /api/passwords/{id}` - Get password with GlutLinks
- `POST /api/passwords` - Create password
- `PUT /api/passwords` - Update password
- `DELETE /api/passwords/{id}` - Delete password
- `GET /api/passwords/export` - Export to CSV

**Generators**
- `GET /api/generators` - List all generators
- `GET /api/generators/{id}` - Get generator
- `POST /api/generators` - Create generator
- `PUT /api/generators` - Update generator
- `DELETE /api/generators/{id}` - Delete generator

**Exports**
- `GET /api/exports` - List all export configurations
- `GET /api/exports/{id}` - Get export configuration
- `POST /api/exports` - Create export configuration
- `PUT /api/exports` - Update export configuration
- `DELETE /api/exports/{id}` - Delete export configuration

**Relays**
- `GET /api/relays` - List all relay configurations
- `GET /api/relays/{id}` - Get relay configuration
- `POST /api/relays` - Create relay configuration
- `PUT /api/relays` - Update relay configuration
- `DELETE /api/relays/{id}` - Delete relay configuration

## Project Structure

```
Glutspeicher/
├── Glutspeicher Server/          # ASP.NET Core server application
│   ├── Context/                  # LiteDB database context
│   ├── Mapping/                  # API endpoint definitions
│   ├── Middleware/               # Custom middleware (Frontend, NoCache)
│   ├── Model/                    # Data models (Password, Generator, Export, Relay)
│   ├── Routing/                  # API routing configuration
│   ├── wwwroot/                  # Static web assets
│   │   ├── static/
│   │   │   ├── css/             # SCSS stylesheets
│   │   │   ├── js/              # JavaScript modules
│   │   │   ├── page/            # SPA page components
│   │   │   └── res/             # Resources (images, icons)
│   │   └── index.html           # Main HTML entry point
│   ├── Program.cs               # Application entry point
│   └── appsettings.json         # Configuration
├── Glutspeicher Client/          # Windows client application
│   ├── Actions/                  # Client action handlers
│   ├── AutoType/                 # AutoType functionality
│   ├── Tausi.NativeWindow/       # Native Windows integration
│   ├── Program.cs               # Client entry point
│   ├── RelaySession.cs          # SSH relay management
│   └── Utils.cs                 # Utility functions
├── docker-compose.yml           # Docker Compose configuration
├── Dockerfile                   # Docker image definition
└── Glutspeicher.sln            # Visual Studio solution
```

## Use Cases

### Personal Password Management
- Store and organize personal credentials securely
- Generate strong passwords with custom rules
- Quick access via modern web interface
- Automatic backups for peace of mind

### Home Lab Administration
- Manage credentials for multiple servers and services
- SSH/RDP connection management with relay support
- TOTP support for 2FA-enabled services
- Self-hosted solution with no external dependencies

### Small Team Collaboration
- Shared password database on internal network
- Organized sections for different projects/clients
- CSV export for integration with other tools
- Audit trail through timestamped backups

### Development & Testing
- Store API keys and service credentials
- Quick password generation for test accounts
- Export functionality for CI/CD integration
- Docker deployment for containerized environments

## What Makes Glutspeicher Unique

1. **True Self-Hosting**: Complete control over your data with no cloud dependencies
2. **Hybrid Architecture**: Combined web and native client for optimal functionality
3. **Advanced Relay System**: Access isolated systems through SSH tunnels
4. **AutoType Integration**: Seamless credential entry on Windows systems
5. **Automatic Backups**: Every change creates a compressed, encrypted backup
6. **Developer-Friendly**: Clean REST API, modern tech stack, open architecture
7. **Zero Vendor Lock-in**: Standard database format, CSV export, open source
8. **Production-Ready**: Windows Service support, Docker images, comprehensive error handling

## Version Information

- **Current Server Version**: 0.5.9
- **Current Client Version**: 0.3.6
- **Target Framework**: .NET 9.0
- **Minimum Requirements**: .NET 9.0 Runtime (server), .NET 9.0 Desktop Runtime (Windows client)

## Links

- **Docker Hub**: https://hub.docker.com/r/melektaus/glutspeicher
- **GitHub Repository**: https://github.com/themelektaus/glutspeicher
- **Server Version**: Downloads available through web interface
- **Client Version**: Downloads available through web interface

## Development

### Debug Mode
- Encryption key automatically set to test value
- Port 80 becomes 5580 on Windows
- Detailed logging enabled
- Server-side includes resolved on every request

### Release Mode
- Automatic Windows Service registration
- Administrator privileges required
- Secure encryption key generation
- Production optimizations enabled

### Contributing
Built with modern .NET practices and clean architecture. The codebase is organized for maintainability with:
- Separation of concerns (API, Middleware, Data)
- Dependency injection
- RESTful API design
- Component-based frontend

## License

This project's license information can be found in the repository.

---

**Glutspeicher** - German for "Gluten Storage", a secure vault for your digital credentials.
