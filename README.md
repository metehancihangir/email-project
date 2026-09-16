# SUBMAIL

SUBMAIL is a newsletter subscription and campaign management application with a React frontend and an ASP.NET Core API. It supports email confirmation, subscriber preferences, an administrator dashboard, campaign analytics, and Gemini-assisted newsletter content.

## Features

- Public subscription with double opt-in email confirmation.
- Confirmation resend controls and rate limiting.
- Subscriber preferences and unsubscribe links.
- JWT-protected administrator login and subscriber management.
- A rich-text newsletter editor, test sends, and campaign history.
- Background email delivery through MailKit.
- Open, click, and feedback tracking.
- A public newsletter archive.
- AI draft generation and scheduled category-based newsletters.

## Technology stack

| Layer | Technologies |
| --- | --- |
| API | ASP.NET Core, C#, .NET 10 |
| Database | MySQL 8, EF Core 9, Pomelo |
| Authentication | JWT bearer tokens and BCrypt |
| Email | MailKit, HTML templates, background queue |
| AI | Gemini HTTP API with configurable model/key fallbacks |
| Frontend | React 19, Vite 8, Tailwind CSS 4, React Router, Axios |
| UI tools | Quill, Framer Motion, Recharts |
| Tests | xUnit, Moq, ASP.NET Core test host, EF Core InMemory |

## Requirements

- .NET **10 SDK**.
- Node.js **22.12 or later**, with npm.
- MySQL **8**.
- An SMTP account for real email delivery.
- A Gemini API key only if using AI features.

## Local setup

### 1. Configure the backend

Clone the repository and open its root directory:

```bash
git clone https://github.com/metehancihangir/email-project.git
cd email-project
```

The API has a configured .NET User Secrets ID. Store local credentials outside the repository. Example commands below use PowerShell; replace all angle-bracketed values:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=emailsubscriber;Uid=<database-user>;Pwd=<database-password>;" --project EmailSubscriber.API
dotnet user-secrets set "Jwt:Secret" "<a-random-signing-key-at-least-32-characters>" --project EmailSubscriber.API
dotnet user-secrets set "Admin:Username" "admin" --project EmailSubscriber.API
```

Create a new administrator password hash with the interactive helper:

```powershell
dotnet run --project HashGen/HashGen.csproj
```

The helper hides password input and prints only a BCrypt hash. Store that hash using single quotes so PowerShell preserves its dollar signs:

```powershell
dotnet user-secrets set "Admin:PasswordHash" '<paste-the-generated-bcrypt-hash>' --project EmailSubscriber.API
```

There is no committed default administrator password or JWT signing key. The API requires a signing key of at least 32 characters.

### 2. Configure optional integrations

For real email delivery:

```powershell
dotnet user-secrets set "Smtp:Host" "<smtp-host>" --project EmailSubscriber.API
dotnet user-secrets set "Smtp:Port" "587" --project EmailSubscriber.API
dotnet user-secrets set "Smtp:User" "<smtp-username>" --project EmailSubscriber.API
dotnet user-secrets set "Smtp:Password" "<smtp-password-or-app-password>" --project EmailSubscriber.API
```

SMTP uses STARTTLS. When `Smtp:Host` or `Smtp:User` is empty, the email service uses mock logging instead of sending mail. Mock mode does not deliver confirmation links to an inbox.

For AI content generation:

```powershell
dotnet user-secrets set "Gemini:ApiKey" "<gemini-api-key>" --project EmailSubscriber.API
```

An optional `Gemini:FallbackApiKey` is supported. Model preferences are configured through `Gemini:Model` and `Gemini:FallbackModels`.

The AI scheduler starts with the API and can send newsletters at configured category times when working credentials and confirmed subscribers are present. Use a separate development database and test mail account while evaluating it.

### 3. Initialize MySQL and run the API

Create the database/user referenced by the connection string, then apply migrations:

```powershell
dotnet restore EmailSubscriber.API/EmailSubscriber.API.csproj
dotnet tool install --global dotnet-ef --version "9.*"
dotnet ef database update --project EmailSubscriber.API
dotnet run --project EmailSubscriber.API --launch-profile http
```

If `dotnet-ef` is already installed, use an EF Core 9-compatible version.

- API: **http://localhost:5117**
- Swagger, in Development: **http://localhost:5117/swagger**
- Health: **http://localhost:5117/health**

The HTTPS launch profile also serves **https://localhost:7157**. Use `dotnet dev-certs https --trust` when running that profile locally.

Development startup includes cleanup of campaigns with certain test subject prefixes and subscribers ending in `@test.com`. Use a disposable development database.

### 4. Run the frontend

In another terminal:

```powershell
cd email-subscriber-client
npm ci
$env:VITE_API_URL = "http://localhost:5117"
npm run dev
```

Open **http://localhost:5173** and visit **/admin/login** for the administrator interface. The frontend API URL is the server origin, **without an /api suffix**.

## Configuration reference

Production deployments can use environment variables with double underscores:

| Setting | Environment variable | Purpose |
| --- | --- | --- |
| `ConnectionStrings:Default` | `ConnectionStrings__Default` | MySQL connection |
| `Jwt:Secret` | `Jwt__Secret` | JWT and tracking-signature key |
| `Admin:Username` | `Admin__Username` | Administrator username |
| `Admin:PasswordHash` | `Admin__PasswordHash` | BCrypt hash |
| `Smtp:Host/Port/User/Password` | `Smtp__Host`, etc. | SMTP connection and credentials |
| `Gemini:ApiKey` | `Gemini__ApiKey` | Primary AI key |
| `Gemini:FallbackApiKey` | `Gemini__FallbackApiKey` | Optional secondary AI key |
| `App:BaseUrl` | `App__BaseUrl` | Frontend URL used in links |
| `App:ApiBaseUrl` | `App__ApiBaseUrl` | API URL used for tracking/assets |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0`, etc. | Permitted frontend origins |

Set forwarded proxy/network settings explicitly when deploying behind a reverse proxy. Use HTTPS for deployed endpoints. Frontend `VITE_*` values are embedded in browser code and must never contain private credentials.

## Repository structure

```text
EmailSubscriber.API/
  Controllers/       Public, admin, and tracking endpoints
  Services/          Subscribers, campaigns, mail, tracking, and AI
  Queue/             Background email queue and worker
  Models/            Subscriber and campaign entities
  Data/              EF Core context
  Migrations/        MySQL schema migrations
  Templates/         Confirmation and welcome emails
EmailSubscriber.Tests/  Unit and in-memory integration tests
email-subscriber-client/ React frontend
HashGen/                Interactive BCrypt hash utility
docs/                   Requirements and development notes
```

## Checks

From the repository root:

```powershell
dotnet test EmailSubscriber.Tests/EmailSubscriber.Tests.csproj -c Release
dotnet build HashGen/HashGen.csproj -c Release
cd email-subscriber-client
npm ci
npm run lint
npm run build
```

The current `EmailSubscriber.slnx` is empty, so use the explicit project paths above. Integration fixtures supply their own temporary signing key, use an in-memory database, and override SMTP/AI credentials.

## Secret handling

Keep credentials in User Secrets or your deployment's secret store. Build outputs and local environment files are ignored because they may contain configuration copies.

The repository includes a [Gitleaks configuration](.gitleaks.toml) that supplements default rules with checks for credentials in `appsettings` files. See [SECURITY.md](SECURITY.md) for rotation and history-cleanup guidance.
