import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { MemoryRouter } from 'react-router-dom';
import AuthRedirect from './AuthRedirect';
import { AuthProvider } from '../context/AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

const TestChild = () => <div>Protected Content</div>;

const renderAuthRedirect = async (
  isAuthenticated: boolean,
  redirectTo: string = '/login',
  isLoading: boolean = false
) => {
  if (isLoading) {
    mockApiClient.mockImplementation(() => new Promise(() => {}));
  } else {
    mockApiClient.mockResolvedValue({
      ok: isAuthenticated,
      json: async () => ({ isAuthenticated, role: isAuthenticated ? 'Buyer' : null }),
    } as Response);
  }

  const result = render(
    <MemoryRouter initialEntries={['/test']}>
      <AuthProvider>
        <AuthRedirect redirectTo={redirectTo}>
          <TestChild />
        </AuthRedirect>
      </AuthProvider>
    </MemoryRouter>
  );

  if (!isLoading) {
    await new Promise(resolve => setTimeout(resolve, 50));
  }

  return result;
};

describe('AuthRedirect Component', () => {
  beforeEach(() => {
    mockApiClient.mockClear();
  });

  test('shows loading state while checking authentication', async () => {
    await renderAuthRedirect(false, '/login', true);

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  test('renders children when authenticated', async () => {
    await renderAuthRedirect(true, '/login');

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  test('redirects to specified path when not authenticated', async () => {
    await renderAuthRedirect(false, '/login');

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  test('uses custom redirectTo path', async () => {
    await renderAuthRedirect(false, '/custom-login');

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  test('renders multiple children when authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Admin' }),
    } as Response);

    render(
      <MemoryRouter initialEntries={['/test']}>
        <AuthProvider>
          <AuthRedirect redirectTo="/login">
            <div>Child 1</div>
            <div>Child 2</div>
            <div>Child 3</div>
          </AuthRedirect>
        </AuthProvider>
      </MemoryRouter>
    );

    await new Promise(resolve => setTimeout(resolve, 50));

    expect(screen.getByText('Child 1')).toBeInTheDocument();
    expect(screen.getByText('Child 2')).toBeInTheDocument();
    expect(screen.getByText('Child 3')).toBeInTheDocument();
  });

  test('preserves children props when authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
    } as Response);

    const ChildWithProps = ({ message }: { message: string }) => (
      <div data-testid="child-with-props">{message}</div>
    );

    render(
      <MemoryRouter initialEntries={['/test']}>
        <AuthProvider>
          <AuthRedirect redirectTo="/login">
            <ChildWithProps message="Welcome Back" />
          </AuthRedirect>
        </AuthProvider>
      </MemoryRouter>
    );

    await new Promise(resolve => setTimeout(resolve, 50));

    expect(screen.getByTestId('child-with-props')).toHaveTextContent('Welcome Back');
  });

  test('redirects with different paths', async () => {
    const paths = ['/login', '/register', '/landing', '/error'];

    for (const path of paths) {
      mockApiClient.mockResolvedValue({
        ok: false,
        json: async () => ({ isAuthenticated: false, role: null }),
      } as Response);

      const { unmount } = render(
        <MemoryRouter initialEntries={['/test']}>
          <AuthProvider>
            <AuthRedirect redirectTo={path}>
              <TestChild />
            </AuthRedirect>
          </AuthProvider>
        </MemoryRouter>
      );

      await new Promise(resolve => setTimeout(resolve, 50));

      expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();

      unmount();
      mockApiClient.mockClear();
    }
  });
});
