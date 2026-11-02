# Event Management System

A RESTful event management API built with ASP.NET Core 8. The system manages users, events, registrations, tickets, ticket payments, and cancellations using SQL Server and Entity Framework Core.

## Features

- User registration and user lookup
- Event creation, update, cancellation, and capacity management
- Online and walk-in event registrations
- Standard and VIP tickets
- Automatic ticket generation after registration
- Ticket payment and cancellation workflows
- Role-based business rules for `User`, `Admin`, and `SuperAdmin`
- Automatic restoration of event capacity when a registration or ticket is cancelled
- Swagger/OpenAPI documentation in the Development environment

## Technology Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- Microsoft SQL Server
- AutoMapper
- Swashbuckle ASP.NET Core

## Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server Express
- Visual Studio 2022/2026 or another .NET-compatible IDE

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/eyetracking0909/Event-API-Project.git
cd Event-API-Project
```

### 2. Configure SQL Server

Update `EventManagementSystem/appsettings.json`, or preferably use user secrets or an environment-specific configuration file:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR_SERVER;Database=EventManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Do not commit passwords, production connection strings, or other secrets to source control.

### 3. Prepare the database

The application expects an `EventManagementDb` SQL Server database containing the tables represented by the Entity Framework Core models. If migrations are available in your local version of the project, apply them with:

```bash
dotnet ef database update --project EventManagementSystem
```

If the database is provisioned separately, update the connection string to point to that database before starting the API.

### 4. Restore, build, and run

```bash
dotnet restore
dotnet build
dotnet run --project EventManagementSystem
```

When running with the Development profile, Swagger is available at:

- `https://localhost:7194/swagger`
- `http://localhost:5211/swagger`

The exact URL may vary depending on the selected launch profile.

## API Overview

The API uses the `/api` route prefix. All request bodies use JSON.

| Area | Endpoint | Purpose |
|---|---|---|
| Users | `GET /api/Users` | List users |
| Users | `GET /api/Users/{id}` | Get a user |
| Users | `POST /api/Users` | Create a user |
| Users | `DELETE /api/Users/{id}?requestedByUserId={id}` | Delete a user; SuperAdmin only |
| Events | `GET /api/Events` | List events |
| Events | `GET /api/Events/{id}` | Get an event |
| Events | `POST /api/Events` | Create an event; Admin or SuperAdmin |
| Events | `PUT /api/Events/{id}` | Update an event; Admin or SuperAdmin |
| Events | `DELETE /api/Events/{id}?requestedByUserId={id}` | Cancel an event; SuperAdmin only |
| Registrations | `GET /api/Registrations` | List registrations |
| Registrations | `GET /api/Registrations/{id}` | Get a registration |
| Registrations | `POST /api/Registrations` | Register a user and create a ticket |
| Registrations | `PUT /api/Registrations/{id}/cancel?cancelledByUserId={id}` | Cancel a registration |
| Tickets | `GET /api/Tickets` | List tickets |
| Tickets | `GET /api/Tickets/{id}` | Get a ticket |
| Tickets | `GET /api/Tickets/code/{ticketCode}` | Find a ticket by code |
| Tickets | `POST /api/Tickets/pay` | Mark a ticket as paid |
| Tickets | `PUT /api/Tickets/{id}/cancel?cancelledByUserId={id}` | Cancel a ticket |

## Example Requests

### Create a user

```http
POST /api/Users
Content-Type: application/json

{
  "firstName": "Alex",
  "lastName": "Morgan",
  "email": "alex.morgan@example.com",
  "password": "change-me",
  "role": "User"
}
```

Valid roles are `User`, `Admin`, and `SuperAdmin`. Email addresses must be unique.

### Create an event

The `createdBy` value must identify an existing Admin or SuperAdmin.

```http
POST /api/Events
Content-Type: application/json

{
  "title": "Tech Conference 2026",
  "description": "A conference for software professionals.",
  "venue": "Main Convention Center",
  "capacity": 200,
  "price": 49.99,
  "registrationStart": "2026-01-01T09:00:00Z",
  "registrationEnd": "2026-05-01T23:59:59Z",
  "eventStart": "2026-05-15T09:00:00Z",
  "eventEnd": "2026-05-15T17:00:00Z",
  "createdBy": 1
}
```

### Register for an event

`quantity` must be between 1 and 10. VIP tickets cost three times the event's base price.

```http
POST /api/Registrations
Content-Type: application/json

{
  "eventId": 1,
  "userId": 2,
  "registrationType": "Online",
  "ticketType": "Standard",
  "quantity": 2
}
```

A successful registration reduces the event's available capacity and returns a generated ticket code. The initial ticket status is `Unpaid`.

### Pay for a ticket

The payer must be the registration owner, an Admin, or a SuperAdmin.

```http
POST /api/Tickets/pay
Content-Type: application/json

{
  "registrationId": 1,
  "paidByUserId": 2
}
```

## Business Rules

- Events start with `AvailableSlots` equal to their capacity.
- An event cannot be registered for after it has been cancelled.
- An event's capacity cannot be reduced below the number of confirmed tickets.
- Cancelling a registration or ticket restores its reserved slots.
- Cancelling a registration also cancels its associated ticket.
- A ticket cannot be paid twice or paid after cancellation.
- Event creation and updates require an Admin or SuperAdmin user ID.
- Event and user deletion require a SuperAdmin user ID. Event deletion is implemented as a soft cancellation; user deletion is permanent.
- The current API uses user IDs and database role values for permission checks. It does not configure ASP.NET Core authentication or JWT authorization middleware.

## Project Structure

```text
EventManagementSystem/
├── Controllers/   API endpoints for users, events, registrations, and tickets
├── Data/          Entity Framework Core DbContext and database configuration
├── DTOs/          Request and response models
├── Models/        Entity models
├── Profiles/      AutoMapper mappings
├── Program.cs     Dependency injection and application pipeline setup
└── appsettings*.json
```

## Development Notes

- Swagger is enabled only when `ASPNETCORE_ENVIRONMENT` is `Development`.
- The default connection string uses Windows authentication and `TrustServerCertificate=True`; review these settings before production deployment.
- Passwords are currently accepted as part of the user registration model. A production deployment should add secure password hashing and authentication before exposing the API publicly.
- The repository currently contains no automated test project; validate endpoint behavior through Swagger or an HTTP client while developing.

## License

No license is currently specified for this repository. Add a license file before distributing the project or accepting external contributions.
