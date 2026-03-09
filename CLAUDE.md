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
    login/              Phone + password login (dark brown hero, card-overlap layout)
    register/           Owner self-registration (topbar + card, two sections)
    activate/           Invite activation via ?token= query param (blue hero)
    cashier/            Order dashboard with card grid, loading/empty states
    coordinator/        Placeholder shell
    branch-manager/     Placeholder shell
    restaurant-manager/ Placeholder shell
    driver/             Placeholder shell
  shared/
    styles/
      _tokens.scss      Design tokens (colors, fonts, spacing, radii, shadows)
      _base.scss        CSS reset + global box-sizing / direction: rtl
      _typography.scss  Global heading/body type styles
      _utilities.scss   Helper classes
      _auth.scss        Auth form patterns (form-stack, input-group, etc.) — also
                        copied into each auth component SCSS for encapsulation
    ui/
      index.ts                  Barrel export for all UI components
      button/                   ui-button (variant, size inputs)
      input/                    ui-input (ControlValueAccessor)
      card/                     ui-card (content projection)
      status-badge/             ui-status-badge (OrderStatus → Arabic label + color)
      loading-spinner/          ui-loading-spinner
      empty-state/              ui-empty-state (icon + message + optional action)
      success-modal/            ui-success-modal (icon, title, message, proceed output)
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

## Frontend styling rules

Angular uses the **Esbuild** builder. Global SCSS (`styles.scss`) compiles correctly but
component-scoped styles take priority. Key rule:

**Auth form styles must be in each component's own SCSS file** — not only in the global
`_auth.scss` — because Esbuild's per-component compilation does not reliably apply global
class rules to component templates in this project's configuration.

Each auth page component (`login`, `register`, `activate`) therefore contains a full local
copy of the shared auth patterns (`.form-stack`, `.input-group`, `.input-field`, `.auth-btn`,
etc.) imported via `@use '../../shared/styles/tokens' as *`.

The `styles.scss` global entry still `@use`s `_auth.scss` so success-modal and any future
non-encapsulated contexts can rely on it.

### SCSS @use path rule
Always use **relative paths** for `@use`. The `stylePreprocessorOptions.includePaths` in
`angular.json` does NOT propagate to individual component SCSS compilations under Esbuild.

```scss
// ✅ Correct
@use '../../shared/styles/tokens' as *;

// ❌ Wrong — breaks at build time
@use 'tokens' as *;
```

## Dev notes
- Docker Desktop must be started manually before running the DB
- API port is 5000 (configured in Properties/launchSettings.json)
- Angular base URL is hardcoded to http://localhost:5000 in ApiClientService
- Azure DevOps NU1900 / NU1301 warnings are from an unrelated corporate NuGet feed — benign, ignore them
- dotnet-ef global tool v8.0.0 is installed
- BCrypt work factor default (10) is used for password hashing
- `ui-success-modal` is used on all three auth pages; it receives `icon`, `iconBg`, `title`,
  `message`, `actionLabel` inputs and emits `(proceed)` which triggers role-based navigation
