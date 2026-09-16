import { afterEach, describe, expect, it } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MockedProvider } from '@apollo/client/testing/react';
import { LOGIN_MUTATION } from '../graphql/mutations';
import { AuthProvider, useAuth } from './AuthContext';

function LoginProbe() {
  const { user, isAuthenticated, login, logout } = useAuth();
  return (
    <div>
      <div data-testid="status">{isAuthenticated ? user?.displayName : 'signed out'}</div>
      <button onClick={() => login('jane@example.com', 'hunter2')}>Log in</button>
      <button onClick={logout}>Log out</button>
    </div>
  );
}

const loginMock = {
  request: {
    query: LOGIN_MUTATION,
    variables: { email: 'jane@example.com', password: 'hunter2' },
  },
  result: {
    data: {
      login: {
        token: 'test-token',
        user: { id: 'u-1', email: 'jane@example.com', displayName: 'Jane Alvarez' },
      },
    },
  },
};

afterEach(() => {
  localStorage.clear();
});

describe('AuthProvider', () => {
  it('starts signed out when nothing is stored', () => {
    render(
      <MockedProvider mocks={[loginMock]}>
        <AuthProvider>
          <LoginProbe />
        </AuthProvider>
      </MockedProvider>,
    );

    expect(screen.getByTestId('status')).toHaveTextContent('signed out');
  });

  it('stores the token and user after a successful login', async () => {
    const user = userEvent.setup();
    render(
      <MockedProvider mocks={[loginMock]}>
        <AuthProvider>
          <LoginProbe />
        </AuthProvider>
      </MockedProvider>,
    );

    await user.click(screen.getByText('Log in'));

    await waitFor(() => expect(screen.getByTestId('status')).toHaveTextContent('Jane Alvarez'));
    expect(localStorage.getItem('pactly_token')).toBe('test-token');
  });

  it('clears stored auth state on logout', async () => {
    const user = userEvent.setup();
    render(
      <MockedProvider mocks={[loginMock]}>
        <AuthProvider>
          <LoginProbe />
        </AuthProvider>
      </MockedProvider>,
    );

    await user.click(screen.getByText('Log in'));
    await waitFor(() => expect(screen.getByTestId('status')).toHaveTextContent('Jane Alvarez'));

    await user.click(screen.getByText('Log out'));

    expect(screen.getByTestId('status')).toHaveTextContent('signed out');
    expect(localStorage.getItem('pactly_token')).toBeNull();
  });
});
