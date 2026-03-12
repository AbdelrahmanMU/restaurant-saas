# Restaurant SaaS Platform

A multi-tenant SaaS platform that helps restaurants manage orders, staff, and delivery drivers.

## Product goals
- Simple, mobile-first Arabic UI for non-technical restaurant staff
- Role-based dashboards: each role sees only what they need
- Real-time order lifecycle tracking from cashier acceptance to driver delivery
- Owner self-service onboarding with invite-based staff activation

## Roles

| Role | Description |
|---|---|
| Owner | Full access. Creates the restaurant account. Can manage all user roles. |
| RestaurantManager | Manages restaurant-level settings and staff. |
| BranchManager | Manages one branch: staff, shifts, settings. |
| Cashier | Accepts orders, manages table/pickup orders. |
| Coordinator | Assigns drivers, tracks dispatch. |
| Driver | Views and updates their active delivery. |

Users can hold **multiple roles** simultaneously via the `UserRoles` join table.

## Onboarding flows

**Owner registration** (`/register`)
Owner fills in: name, phone, password, restaurant name.
System creates: Restaurant + first Branch ("الفرع الرئيسي") + Owner user + Owner UserRole.

**Staff invite activation** (`/activate?token=<uuid>`)
Admin creates a UserInvite record (phone, role, branch, expiry).
Staff receives the link, enters their name, phone, and sets a password.
System creates the User + UserRole and marks the invite as accepted.

**Login** (`/login`)
All users log in with phone number + password.
On success: JWT (7 days) + roles[] + branchId + restaurantId returned.
- Single-role users → auto-committed, redirect to dashboard
- Multi-role users → `/select-role` picker, then redirect to chosen dashboard

## Multi-role navigation rules

- `activeRole` in localStorage is the sole source of truth for current session context.
- `auth.goToDashboard()` must be used for all "go back to my dashboard" navigation — never hardcode a route.
- `auth.redirectByRole()` is called once after login/register/activate.
- `authGuard` redirects multi-role users without a committed `activeRole` to `/select-role`.
- `storeSession()` clears `activeRole` on every fresh login.

## Order workflow

```
Customer places order
       ↓
[Cashier]       PendingAcceptance → Accepted
       ↓
[Kitchen]       Accepted → Preparing
       ↓
[Coordinator]   Preparing → ReadyForDispatch → PendingHandover
       ↓
[Driver]        PendingHandover → PickedUp → Delivered
       ↓
[System]        Delivered → Completed
```

- Any order can be moved to **Cancelled** at any stage.
- Every status transition is recorded in **OrderEventLog** (who, when, from, to).
- A driver can hold only **one active order** at a time (enforced by DriverActiveOrder).

## Multi-tenancy

- Every Restaurant is isolated. Branches belong to one Restaurant.
- Users are scoped to a Restaurant and optionally to a Branch.
- JWT claims carry `restaurant_id` and `branch_id` for fast authorization checks.

## Current implementation state

### Backend — Done
- Full backend structure: Domain/Entities, Domain/Enums, DTOs, Services/Interfaces, Validators
- Phone + password authentication with BCrypt hashing
- `POST /auth/login` — returns JWT with multiple role claims
- `POST /auth/register-owner` — creates Restaurant + default Branch + Owner user + Owner UserRole
- `POST /auth/activate-invite` — validates UserInvite token, creates User + UserRole, marks invite used
- JWT: userId, one claim per role, fullName, restaurant_id, branch_id. Expires 7 days.
- FluentValidation on all auth request DTOs
- `GET /me` — returns current user info (auth required)
- `GET /health` — health check
- **Multi-role**: `UserRoles` join table replaces single `Users.Role` column. Migration applied.
- **Staff invites**: `POST /invites` — `[Authorize(Roles = "Owner,BranchManager")]`, returns activation link
- **User management** (Owner only):
  - `GET /users` — list all users in restaurant
  - `GET /users/{id}` — user detail with `roleEntries[]` (includes GUID IDs for removal)
  - `POST /users/{id}/roles` — add role (enum-validated, duplicate-checked)
  - `DELETE /users/{id}/roles/{roleId}` — remove role (blocks removing last role)

### Frontend — Done
- Angular 17 standalone components, lazy-loaded routes
- Design system SCSS: `_tokens.scss`, `_base.scss`, `_typography.scss`, `_utilities.scss`, `_auth.scss`
- 8 shared UI components: `ui-button`, `ui-input`, `ui-card`, `ui-status-badge`, `ui-loading-spinner`, `ui-empty-state`, `ui-success-modal`, `ui-dashboard-layout`
- Auth pages: Login, Register, Activate (fully styled, success modal on submit)
- Cashier orders dashboard: card grid, status badges, mock data
- **Select role page** (`/select-role`): hero + role cards, guarded by `selectRoleGuard`
- **Branch manager dashboard**: links to staff invite form
- **Restaurant manager dashboard**: stats cards (mock), staff list (mock), Owner-only "Manage Users" card
- **Staff invite page** (`/branch-manager/staff` + `/restaurant-manager/staff`): shared `StaffComponent`, role-aware back navigation via `auth.goToDashboard()`
- **Owner users page** (`/owner/users`): user list + slide-up panel, add/remove roles, cannot remove last role
- Guards: `authGuard` (login + activeRole enforcement), `selectRoleGuard` (picker flow)
- Services: `auth.service.ts`, `api-client.service.ts`, `order.service.ts`, `invite.service.ts`, `user-management.service.ts`

### Frontend — Placeholder shells (routes exist, no content)
- Coordinator, Driver pages

### Backend — Mock / not yet wired
- Orders: in-memory mock in `OrderService._mockOrders` — not reading from DB
- `PATCH /orders/:id/accept` not yet implemented

### Next priorities
1. Wire orders to real DB (replace mock OrderService with EF queries scoped to branch)
2. Implement `PATCH /orders/:id/accept` (PendingAcceptance → Accepted + OrderEventLog entry)
3. Build coordinator dispatch board (assign driver, mark ReadyForDispatch → PendingHandover)
4. Build Driver dashboard (view active delivery, update status)
5. Real-time order updates (SignalR or polling)
6. Wire real staff list in restaurant-manager dashboard (replace mock array with `GET /users`)

## Key business rules
- Drivers cannot hold more than one active order simultaneously
- All order status transitions must be persisted in OrderEventLog
- Invite tokens expire (ExpiresAt on UserInvite) and can only be used once (IsAccepted flag)
- Passwords are hashed with BCrypt (never stored in plain text)
- The platform is multi-tenant: all DB queries must be scoped to the correct Restaurant/Branch
- Owners cannot have their last role removed via the role management UI
- `activeRole` is cleared on every fresh login — multi-role users must re-select each session
