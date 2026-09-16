# Pactly frontend

React, TypeScript and Vite, talking to the Pactly GraphQL API through Apollo Client. Material UI is used for components, styled through a custom theme rather than the defaults.

## Local development

The real backend needs MongoDB and the .NET SDK, neither of which is required to work on the frontend. Instead, run the bundled mock GraphQL server:

```bash
npm install
npm run mock-api    # serves a mock schema on http://localhost:5236/graphql
npm run dev          # serves the app on http://localhost:5173
```

Point `VITE_GRAPHQL_URL` at a real deployment when you want to test against it instead.

## Scripts

- `npm run dev` - start the Vite dev server
- `npm run build` - type check and build for production
- `npm run test` - run the vitest suite
- `npm run lint` - run oxlint
- `npm run mock-api` - run the zero-dependency mock GraphQL server used for local UI work
