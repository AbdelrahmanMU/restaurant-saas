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

## Current implementation state (as of session end)

### Done
- Full backend structure: Domain/Entities, Domain/Enums, DTOs, Services/Interfaces, Validators
- Phone + password authentication with BCrypt hashing
- Owner self-registration endpoint (creates Restaurant + Branch + User)
- Invite activation endpoint (validates UserInvite, creates User)
- JWT with role, branchId, restaurantId claims
- FluentValidation on all auth request DTOs
- Cashier orders dashboard (mock data): card grid, status badges, accept button, loading/empty states
- Angular pages: login, register, activate, cashier, coordinator, branch-manager, restaurant-manager, driver
- All Angular components use external .html and .scss files (no inline templates)

### Mock / placeholder
- Orders data is in-memory mock (OrderService._mockOrders) — not yet reading from DB
- Accept order button shows alert placeholder — PATCH /orders/:id/accept not yet implemented
- Coordinator, BranchManager, RestaurantManager, Driver pages are placeholder shells

### Next priorities
1. Wire orders to real DB (replace mock OrderService with EF queries scoped to branch)
2. Implement PATCH /orders/:id/accept (PendingAcceptance → Accepted)
3. Build coordinator dispatch board (assign driver, mark ready)
4. Implement invite creation endpoint (POST /invites) for managers
5. Real-time order updates (SignalR or polling)

## Key business rules
- Drivers cannot hold more than one active order simultaneously
- All order status transitions must be persisted in OrderEventLog
- Invite tokens expire (ExpiresAt on UserInvite) and can only be used once (IsAccepted flag)
- Passwords are hashed with BCrypt (never stored in plain text)
- The platform is multi-tenant: all DB queries must be scoped to the correct Restaurant/Branch
