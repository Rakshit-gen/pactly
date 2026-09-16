# Pactly

Pactly is a self-serve commerce app for buying and managing a SaaS subscription, where every purchase produces a real agreement: a generated contract, a signature step, and an audit trail of who did what and when.

It's basically the spot where "buy a plan online" and "sign a contract" overlap, built as one flow instead of two separate products bolted together.

## Live

- App: https://pactly-rho.vercel.app
- API: https://pactly-api.onrender.com/graphql

The backend is on Render's free tier, so the first request after a period of inactivity can take up to a minute while the instance spins back up.

## Why this exists

Most subscription checkouts stop at payment. This one treats the purchase itself as an agreement that needs to be drafted, signed, and executed, with proration handled properly when you upgrade or downgrade mid cycle. Picking a plan or add-on walks through:

1. **Checkout** - the cart is turned into an order. If it's a plan change mid-cycle, the unused time on the old plan is credited and the new plan is charged for the remaining days in the period.
2. **Draft** - an agreement is generated from the order, in a `Draft` state, and immediately moved to `AwaitingSignature`.
3. **Sign** - the buyer draws a signature on the agreement.
4. **Execute** - signing moves the agreement straight to `Signed` and then `Executed` in one step, and the plan is applied to the account. Every transition is appended to the agreement's audit trail rather than overwriting anything.

## Stack

- **Backend**: C# / ASP.NET Core 8, GraphQL via HotChocolate, MongoDB, JWT auth, BCrypt password hashing
- **Frontend**: React 19, TypeScript, Vite, Material UI with a custom theme, Apollo Client
- **Tests**: xUnit on the backend (against hand-written in-memory fakes, no database needed), Vitest on the frontend
- **CI**: GitHub Actions runs `dotnet restore/build/test` on every push that touches `backend/`
- **Hosting**: frontend on Vercel, backend on Render (Docker), database on MongoDB Atlas

## Project layout

```
backend/
  src/Pactly.Core/    domain models, repository interfaces, Mongo implementations, business services
  src/Pactly.Api/      GraphQL schema, JWT auth wiring, HTTP entry point
  tests/Pactly.Tests/  unit tests against in-memory fakes of the repositories
frontend/
  src/                 pages, components, Apollo Client setup, GraphQL documents
  mock-server/         zero-dependency mock GraphQL server for frontend-only local dev
```

`Pactly.Core` has no ASP.NET or GraphQL dependencies, so the business logic (proration math, agreement state transitions, checkout rules) is testable in isolation from the API layer.

## Running it locally

### Backend

Needs the .NET 8 SDK and a MongoDB instance (local or an Atlas connection string).

```bash
cd backend
dotnet restore
dotnet run --project src/Pactly.Api
```

Set `Mongo:ConnectionString` and `Jwt:SigningKey` in `appsettings.Development.json` or as environment variables before running. The app fails fast on startup if the signing key is missing.

Run the test suite (no database required, it runs against in-memory fakes):

```bash
dotnet test tests/Pactly.Tests/Pactly.Tests.csproj
```

### Frontend

The frontend doesn't need the real backend running. A small dependency-free mock GraphQL server ships with it for local UI work:

```bash
cd frontend
npm install
npm run mock-api   # serves a mock schema on http://localhost:5236/graphql
npm run dev         # serves the app on http://localhost:5173
```

To point the frontend at a real backend instead, set `VITE_GRAPHQL_URL` to its `/graphql` endpoint.

```bash
npm run test    # vitest
npm run build   # type check + production build
npm run lint    # oxlint
```

## Deployment notes

The backend is deployed from `render.yaml` as a Docker-based Render Blueprint. The database name, JWT issuer, and JWT audience are fixed in the blueprint; the Mongo connection string, JWT signing key, and allowed CORS origin are set as secrets in the Render dashboard rather than committed.

The frontend is a static Vite build on Vercel, with `VITE_GRAPHQL_URL` set to the Render backend's `/graphql` URL as a Vercel environment variable.
