# Deployment Guide

## Architecture

```
Cloudflare Pages (Angular SPA)
        ↕ HTTPS
  Render Web Service (.NET 8 API / Docker)
        ↕ TLS
  Neon PostgreSQL (serverless Postgres)
```

All three tiers are on free plans.

---

## Prerequisites

- GitHub account (Cloudflare Pages deploys from Git)
- [Render](https://render.com) account
- [Neon](https://neon.tech) account
- [Cloudflare](https://pages.cloudflare.com) account
- `dotnet ef` tool installed locally for running migrations

---

## Step 1 — Neon Database Setup

1. Sign in to [neon.tech](https://neon.tech) → **New Project**
2. Choose a region close to your Render service (e.g. US East)
3. Project name: `restaurant-saas`
4. After creation, go to **Connection Details** → select **Npgsql / ADO.NET**
5. Copy the connection string — it looks like:

```
Host=ep-xxx.us-east-2.aws.neon.tech;Port=5432;Database=neondb;Username=xxx;Password=xxx;Ssl Mode=Require;Trust Server Certificate=true
```

6. Run migrations from your local machine (point at Neon, not local Docker):

```powershell
cd apps/api
$env:ConnectionStrings__Default = "Host=ep-xxx...;...;Ssl Mode=Require;Trust Server Certificate=true"
dotnet ef database update
```

---

## Step 2 — Backend Deployment (Render)

### 2a. First deploy

1. Push this repository to GitHub
2. Sign in to [render.com](https://render.com) → **New** → **Web Service**
3. Connect your GitHub repo
4. Render auto-detects `render.yaml` — click **Apply**

   If you prefer manual setup:
   - **Environment**: Docker
   - **Dockerfile path**: `./Dockerfile`
   - **Docker context**: `.` (repo root)

### 2b. Set environment variables in Render dashboard

Go to your service → **Environment** tab and add:

| Key | Value |
|-----|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Default` | *(Neon connection string from Step 1)* |
| `Jwt__Key` | *(strong random string, min 32 chars)* |
| `Jwt__Issuer` | `restaurant-saas` |
| `Jwt__Audience` | `restaurant-saas-clients` |
| `Frontend__BaseUrl` | *(your Cloudflare Pages URL, set after Step 3)* |
| `PORT` | `10000` |

### 2c. Note your API URL

After deploy completes, Render gives you a URL like:
```
https://restaurant-saas-api.onrender.com
```

Save this — you need it in Step 3.

### Health check

```
GET https://restaurant-saas-api.onrender.com/health
→ { "status": "healthy", "timestamp": "..." }
```

---

## Step 3 — Frontend Deployment (Cloudflare Pages)

### 3a. Update the production API URL

Edit `apps/web/src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://restaurant-saas-api.onrender.com', // your Render URL
};
```

Commit and push this change.

### 3b. Create Cloudflare Pages project

1. Sign in to [pages.cloudflare.com](https://pages.cloudflare.com) → **Create a project** → **Connect to Git**
2. Select your GitHub repo
3. Configure build:

| Setting | Value |
|---------|-------|
| **Framework preset** | Angular |
| **Build command** | `npm run build` |
| **Build output directory** | `dist/web/browser` |
| **Root directory** | `apps/web` |

4. Click **Save and Deploy**

### SPA routing

The `_redirects` file at `src/_redirects` is automatically included in the build output and tells Cloudflare Pages to serve `index.html` for all routes:
```
/* /index.html 200
```

No additional configuration needed.

### 3c. Update CORS on Render

After Cloudflare gives you a domain (e.g. `https://restaurant-saas.pages.dev`), go back to Render and set:

```
Frontend__BaseUrl = https://restaurant-saas.pages.dev
```

Then trigger a redeploy on Render.

---

## Step 4 — Post-Deploy Verification

### Checklist

- [ ] `GET /health` returns `{ "status": "healthy" }` (Render URL)
- [ ] Cloudflare Pages URL loads the Angular app
- [ ] Login page renders and accepts credentials
- [ ] Login returns a JWT (check Network tab — no CORS errors)
- [ ] Dashboard loads after login
- [ ] Menu sections / products load (proves DB connection works)
- [ ] Invite flow works end-to-end
- [ ] Logout and re-login works (JWT expiry/storage)
- [ ] Direct URL navigation works (SPA routing — no 404 on refresh)

---

## Environment Variables Reference

### Backend (Render)

| Variable | Example / Notes |
|----------|----------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Default` | Neon Npgsql connection string with `Ssl Mode=Require` |
| `Jwt__Key` | Random secret ≥ 32 chars |
| `Jwt__Issuer` | `restaurant-saas` |
| `Jwt__Audience` | `restaurant-saas-clients` |
| `Frontend__BaseUrl` | `https://your-app.pages.dev` (no trailing slash) |
| `PORT` | `10000` |

### Frontend (Cloudflare Pages)

Angular environment values are embedded at build time via `environment.prod.ts` — no runtime env vars are needed in Cloudflare Pages. The only value to configure is `apiUrl` in that file.

---

## Migrations

Migrations are **not** auto-applied on startup. Run them manually whenever you add a new EF migration:

```powershell
# From local machine, pointing at Neon
cd apps/api
$env:ConnectionStrings__Default = "Host=ep-xxx...;Ssl Mode=Require;Trust Server Certificate=true"
dotnet ef database update
```

Or use a Render one-off job (Shell) to run migrations from the deployed container:

```bash
dotnet ef database update
```

---

## Local Development (unchanged)

```powershell
# Start local DB
cd infra/docker && docker compose up -d

# Run API
cd apps/api && dotnet run

# Run frontend
cd apps/web && npm start
```

Local dev uses `environment.ts` (not `environment.prod.ts`) and connects to `localhost:5000` automatically.

---

## Free Plan Limitations

| Service | Limitation |
|---------|------------|
| Render (free) | Spins down after 15 min inactivity; cold start ~30s |
| Neon (free) | 0.5 GB storage, compute pauses after inactivity |
| Cloudflare Pages (free) | 500 builds/month, unlimited requests |
