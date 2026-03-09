# CLAUDE.md

Restaurant SaaS development instructions.

## Environment
- Windows
- PowerShell terminal (use PowerShell commands only)
- Node via nvm
- Docker Desktop

## Stack

### Backend
- .NET 8 Web API
- EF Core 8 + Npgsql (PostgreSQL)
- FluentValidation 11 (auto-validation via AddFluentValidationAutoValidation)
- BCrypt.Net-Next 4 (password hashing)

### Frontend
- Angular 17 standalone components
- RTL Arabic UI (direction: rtl on all pages)
- SCSS per component (no inline styles or templates)

## Important rules
- Use PowerShell commands only
- Keep controllers thin — no business logic, only call service + return result
- Business logic lives in Services/
- Always use DTOs — never expose entities directly
- Generate an EF migration whenever the domain model changes
- All components must use templateUrl and styleUrl (no inline template/styles)
- Validators live in Validators/{Feature}/ and are auto-registered via AddValidatorsFromAssemblyContaining<Program>()

## Backend structure

```
apps/api/
  Controllers/          Thin controllers, one action per endpoint
  Data/                 AppDbContext.cs
  Domain/
    Entities/           One file per entity class
    Enums/              One file per enum
  DTOs/
    Auth/               AuthDtos.cs
    Orders/             OrderDtos.cs
  Services/
    Interfaces/         ITokenService, IOrderService, IAuthService
    AuthService.cs      Login, RegisterOwner, ActivateInvite
    TokenService.cs     JWT generation (includes branch_id, restaurant_id claims)
    OrderService.cs     Mock in-memory orders (replace with DB later)
  Validators/
    Auth/               LoginRequestValidator, RegisterOwnerRequestValidator, ActivateInviteRequestValidator
  Migrations/           EF Core migration files
```

## Frontend structure

```
apps/web/src/app/
  app.component.ts/html/scss
  app.routes.ts
  app.config.ts
  core/
    services/           auth.service.ts, api-client.service.ts, order.service.ts
    guards/             auth.guard.ts
  pages/
    login/              Phone + password login (blue gradient card)
    register/           Owner self-registration (green gradient card)
    activate/           Invite activation via ?token= query param (purple card)
    cashier/            Order dashboard with card grid, loading/empty states
    coordinator/
    branch-manager/
    restaurant-manager/
    driver/
  shared/               Reserved for future reusable components
```

## Authentication

Auth is phone + password (no OTP in MVP).

Three entry points:
- `POST /auth/login` — existing users (phone + password → JWT)
- `POST /auth/register-owner` — owner self-registration (creates Restaurant + Branch + User)
- `POST /auth/activate-invite` — staff activation via invite token (UserInvite.Id as GUID in ?token= query param)

JWT contains: userId, role, fullName, branch_id (optional), restaurant_id (optional). Expires in 7 days.

## Order lifecycle

```
PendingAcceptance → Accepted → Preparing → ReadyForDispatch → PendingHandover → PickedUp → Delivered → Completed
                                                                                                        Cancelled (any stage)
```

All status transitions must be logged in OrderEventLog.

## Dev commands

```powershell
# Start database
cd infra/docker && docker compose up -d

# Apply migrations
cd apps/api && dotnet ef database update

# Run API (listens on http://localhost:5000)
cd apps/api && dotnet run

# Run frontend (http://localhost:4200)
cd apps/web && npm start
```

## Dev notes
- Docker Desktop must be started manually before running the DB
- API port is 5000 (configured in Properties/launchSettings.json)
- Angular base URL is hardcoded to http://localhost:5000 in ApiClientService
- Azure DevOps NU1900 / NU1301 warnings are from an unrelated corporate NuGet feed — benign, ignore them
- dotnet-ef global tool v8.0.0 is installed
- BCrypt work factor default (10) is used for password hashing
