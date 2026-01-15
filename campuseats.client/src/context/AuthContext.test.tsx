import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { AuthProvider, useAuth } from './AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const TestComponent = () => {
  const { isAuthenticated, userRole, isLoading, checkAuthStatus } = useAuth();
  
  return (
    <div>
      <div data-testid="auth-status">{isAuthenticated ? 'authenticated' : 'not-authenticated'}</div>
      <div data-testid="user-role">{userRole || 'no-role'}</div>
      <div data-testid="loading-status">{isLoading ? 'loading' : 'loaded'}</div>
      <button onClick={checkAuthStatus}>Check Auth</button>
    </div>
  );
};

describe('AuthContext', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should throw error when useAuth is used outside AuthProvider', () => {
    const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
    
    expect(() => render(<TestComponent />)).toThrow('useAuth must be used within an AuthProvider');
    
    consoleError.mockRestore();
  });

  it('should provide initial loading state', () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    expect(screen.getByTestId('loading-status')).toHaveTextContent('loading');
  });

  it('should set authenticated state when API returns successful response', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
    } as Response);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('authenticated');
    });
    
    expect(screen.getByTestId('user-role')).toHaveTextContent('Buyer');
    expect(screen.getByTestId('loading-status')).toHaveTextContent('loaded');
  });

  it('should set not authenticated when API returns unsuccessful response', async () => {
    mockApiClient.mockResolvedValue({
      ok: false,
    } as Response);

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('not-authenticated');
    });
    
    expect(screen.getByTestId('user-role')).toHaveTextContent('no-role');
    expect(screen.getByTestId('loading-status')).toHaveTextContent('loaded');
  });

  it('should handle API errors gracefully', async () => {
    mockApiClient.mockRejectedValue(new Error('Network error'));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('not-authenticated');
    });
    
    expect(screen.getByTestId('user-role')).toHaveTextContent('no-role');
    expect(screen.getByTestId('loading-status')).toHaveTextContent('loaded');
  });

  it('should allow manual checkAuthStatus call', async () => {
    mockApiClient.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    const { rerender } = render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('not-authenticated');
    });

    mockApiClient.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Admin' }),
    } as Response);

    const button = screen.getByRole('button', { name: /check auth/i });
    button.click();

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('authenticated');
      expect(screen.getByTestId('user-role')).toHaveTextContent('Admin');
    });
  });

  it('should handle response with no role', async () => {
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
      expect(screen.getByTestId('auth-status')).toHaveTextContent('authenticated');
    });
    
    expect(screen.getByTestId('user-role')).toHaveTextContent('no-role');
  });
});
