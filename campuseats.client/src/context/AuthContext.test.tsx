import React from 'react';
import { render, screen, waitFor, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import { AuthProvider, useAuth } from './AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

// Test component to use the AuthContext
const TestComponent: React.FC = () => {
  const { isAuthenticated, userRole, isLoading, checkAuthStatus } = useAuth();

  return (
    <div>
      <div data-testid="is-authenticated">{String(isAuthenticated)}</div>
      <div data-testid="user-role">{userRole || 'null'}</div>
      <div data-testid="is-loading">{String(isLoading)}</div>
      <button onClick={checkAuthStatus} data-testid="check-auth">
        Check Auth
      </button>
    </div>
  );
};

describe('AuthContext', () => {
  const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

  beforeEach(() => {
    mockApiClient.mockClear();
  });

  test('throws error when useAuth is used outside AuthProvider', () => {
    // Suppress console.error for this test
    const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    expect(() => {
      render(<TestComponent />);
    }).toThrow('useAuth must be used within an AuthProvider');

    consoleSpy.mockRestore();
  });

  test('initializes with default values and checks auth on mount', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
    } as Response);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    // Initially loading
    expect(screen.getByTestId('is-loading')).toHaveTextContent('true');

    // Wait for auth check to complete
    await waitFor(() => {
      expect(screen.getByTestId('is-loading')).toHaveTextContent('false');
    });

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('true');
    expect(screen.getByTestId('user-role')).toHaveTextContent('Buyer');
    expect(mockApiClient).toHaveBeenCalledWith('/api/user/check-auth');
  });

  test('sets isAuthenticated to false when auth check fails', async () => {
    mockApiClient.mockResolvedValue({
      ok: false,
    } as Response);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('is-loading')).toHaveTextContent('false');
    });

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('false');
    expect(screen.getByTestId('user-role')).toHaveTextContent('null');
  });

  test('handles network errors gracefully', async () => {
    mockApiClient.mockRejectedValue(new Error('Network error'));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('is-loading')).toHaveTextContent('false');
    });

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('false');
    expect(screen.getByTestId('user-role')).toHaveTextContent('null');
  });

  test('checkAuthStatus can be called manually', async () => {
    mockApiClient
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ isAuthenticated: false, role: null }),
      } as Response)
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ isAuthenticated: true, role: 'Admin' }),
      } as Response);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('is-loading')).toHaveTextContent('false');
    });

    expect(screen.getByTestId('is-authenticated')).toHaveTextContent('false');

    // Manually check auth status
    act(() => {
      screen.getByTestId('check-auth').click();
    });

    await waitFor(() => {
      expect(screen.getByTestId('is-authenticated')).toHaveTextContent('true');
      expect(screen.getByTestId('user-role')).toHaveTextContent('Admin');
    });

    expect(mockApiClient).toHaveBeenCalledTimes(2);
  });

  test('handles different user roles', async () => {
    const roles = ['Buyer', 'Kitchen', 'Admin'];

    for (const role of roles) {
      mockApiClient.mockResolvedValue({
        ok: true,
        json: async () => ({ isAuthenticated: true, role }),
      } as Response);

      const { unmount } = render(
        <AuthProvider>
          <TestComponent />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(screen.getByTestId('is-loading')).toHaveTextContent('false');
      });

      expect(screen.getByTestId('user-role')).toHaveTextContent(role);

      unmount();
      mockApiClient.mockClear();
    }
  });

  test('sets role to null when not provided in response', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true }),
    } as Response);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('is-loading')).toHaveTextContent('false');
    });

    expect(screen.getByTestId('user-role')).toHaveTextContent('null');
  });
});
