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
| Owner | Full access. Creates the restaurant account. |
| RestaurantManager | Manages restaurant-level settings and staff. |
| BranchManager | Manages one branch: staff, shifts, settings. |
| Cashier | Accepts orders, manages table/pickup orders. |
| Coordinator | Assigns drivers, tracks dispatch. |
| Driver | Views and updates their active delivery. |

## Onboarding flows

**Owner registration** (`/register`)
Owner fills in: name, phone, password, restaurant name.
System creates: Restaurant + first Branch ("الفرع الرئيسي") + Owner user.

**Staff invite activation** (`/activate?token=<uuid>`)
Admin creates a UserInvite record (phone, role, branch, expiry).
Staff receives the link, enters their name, phone, and sets a password.
System creates the User and marks the invite as accepted.

**Login** (`/login`)
All users log in with phone number + password.
On success: JWT (7 days) + role + branchId + restaurantId returned.
Frontend redirects to the role's dashboard automatically.

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
- `POST /auth/login` — returns JWT on valid credentials
- `POST /auth/register-owner` — creates Restaurant + default Branch ("الفرع الرئيسي") + Owner user
- `POST /auth/activate-invite` — validates UserInvite token (GUID), creates User, marks invite used
- JWT includes: userId, role, fullName, restaurant_id, branch_id. Expires in 7 days.
- FluentValidation on all auth request DTOs
- `GET /me` — returns current user info (auth required)
- `GET /health` — health check endpoint

### Frontend — Done
- Angular 17 standalone components, lazy-loaded routes, `authGuard`
- Design system SCSS: `_tokens.scss`, `_base.scss`, `_typography.scss`, `_utilities.scss`, `_auth.scss`
- 7 shared UI components: `ui-button`, `ui-input`, `ui-card`, `ui-status-badge`, `ui-loading-spinner`, `ui-empty-state`, `ui-success-modal`
- Auth pages fully styled to approved prototype:
  - **Login** (`/login`): dark brown hero (gradient `#2C2420 → #4A3830`), card-overlap layout, phone + password fields, show/hide toggle, success modal
  - **Register** (`/register`): topbar with back arrow, two-section form (personal + restaurant), success modal (🎊)
  - **Activate** (`/activate?token=`): blue hero (gradient `#1E3A5F → #2E4F7A`), tokenMissing warning state, success modal (🚀)
- Cashier orders dashboard: card grid, status badges, accept button, loading/empty states (mock data)
- All page layouts use `.auth-shell` centering wrapper (`max-width: 420px` phone-frame, `#E8E0D5` body background)
- All auth pages contain auth form styles directly in component SCSS (Esbuild encapsulation workaround)

### Frontend — Placeholder shells
- Coordinator, BranchManager, RestaurantManager, Driver pages (routes exist, no content)

### Backend — Mock / not yet wired
- Orders: in-memory mock in `OrderService._mockOrders` — not reading from DB
- `PATCH /orders/:id/accept` not yet implemented (button shows alert placeholder)

### Next priorities
1. Wire orders to real DB (replace mock OrderService with EF queries scoped to branch)
2. Implement `PATCH /orders/:id/accept` (PendingAcceptance → Accepted + OrderEventLog entry)
3. Build coordinator dispatch board (assign driver, mark ReadyForDispatch → PendingHandover)
4. Implement `POST /invites` — manager creates invite link for staff
5. Real-time order updates (SignalR or polling)
6. Build BranchManager, RestaurantManager, Driver dashboards

## Key business rules
- Drivers cannot hold more than one active order simultaneously
- All order status transitions must be persisted in OrderEventLog
- Invite tokens expire (ExpiresAt on UserInvite) and can only be used once (IsAccepted flag)
- Passwords are hashed with BCrypt (never stored in plain text)
- The platform is multi-tenant: all DB queries must be scoped to the correct Restaurant/Branch
