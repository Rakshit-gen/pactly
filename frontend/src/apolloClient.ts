import { ApolloClient, InMemoryCache, HttpLink, from } from '@apollo/client';
import { setContext } from '@apollo/client/link/context';

const TOKEN_STORAGE_KEY = 'pactly_token';

const httpLink = new HttpLink({
  uri: import.meta.env.VITE_GRAPHQL_URL ?? 'http://localhost:5236/graphql',
});

const authLink = setContext((_, { headers }) => {
  const token = localStorage.getItem(TOKEN_STORAGE_KEY);
  return {
    headers: {
      ...headers,
      ...(token ? { authorization: `Bearer ${token}` } : {}),
    },
  };
});

export const apolloClient = new ApolloClient({
  link: from([authLink, httpLink]),
  cache: new InMemoryCache(),
});

export { TOKEN_STORAGE_KEY };
