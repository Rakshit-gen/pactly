# Pactly

Pactly is a self-serve commerce app for buying and managing a SaaS subscription, where every purchase produces a real agreement: a generated contract, a signature step, and an audit trail of who did what and when.

It's basically the spot where "buy a plan online" and "sign a contract" overlap, built as one flow instead of two separate products bolted together.

## Why this exists

Most subscription checkouts stop at payment. This one treats the purchase itself as an agreement that needs to be drafted, signed, and executed, with proration handled properly when you upgrade or downgrade mid cycle.

## Stack

- Backend: C# / ASP.NET Core, GraphQL via HotChocolate, MongoDB
- Frontend: React, TypeScript, Material UI, Apollo Client
- Tests: xUnit on the backend, Vitest on the frontend
- CI: GitHub Actions
- Hosting: frontend on Vercel, backend on Render

More detail on setup and architecture goes in the docs as the project fills out.
