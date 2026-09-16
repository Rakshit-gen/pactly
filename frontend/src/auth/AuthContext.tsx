import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { useMutation } from '@apollo/client/react';
import { LOGIN_MUTATION, REGISTER_MUTATION } from '../graphql/mutations';
import { apolloClient, TOKEN_STORAGE_KEY } from '../apolloClient';
import type { AuthUser } from '../types';

const USER_STORAGE_KEY = 'pactly_user';

interface AuthPayloadData {
  token: string;
  user: AuthUser;
}

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function readStoredUser(): AuthUser | null {
  const stored = localStorage.getItem(USER_STORAGE_KEY);
  if (!stored) {
    return null;
  }
  try {
    return JSON.parse(stored) as AuthUser;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(readStoredUser);
  const [loginMutation, { loading: loginLoading }] = useMutation<{ login: AuthPayloadData }>(LOGIN_MUTATION);
  const [registerMutation, { loading: registerLoading }] = useMutation<{ register: AuthPayloadData }>(
    REGISTER_MUTATION,
  );

  const persist = (token: string, authUser: AuthUser) => {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(authUser));
    setUser(authUser);
  };

  const login = async (email: string, password: string) => {
    const { data } = await loginMutation({ variables: { email, password } });
    if (data?.login) {
      persist(data.login.token, data.login.user);
    }
  };

  const register = async (email: string, password: string, displayName: string) => {
    const { data } = await registerMutation({ variables: { email, password, displayName } });
    if (data?.register) {
      persist(data.register.token, data.register.user);
    }
  };

  const logout = () => {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(USER_STORAGE_KEY);
    setUser(null);
    void apolloClient.clearStore();
  };

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      loading: loginLoading || registerLoading,
      login,
      register,
      logout,
    }),
    [user, loginLoading, registerLoading],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
