import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { MemoryRouter } from 'react-router-dom';
import PublicRoute from './PublicRoute';
import { AuthProvider } from '../context/AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

const TestChild = () => <div>Public Content</div>;

const renderPublicRoute = async (isAuthenticated: boolean, isLoading: boolean = false) => {
  if (isLoading) {
    mockApiClient.mockImplementation(() => new Promise(() => {}));
  } else {
    mockApiClient.mockResolvedValue({
      ok: isAuthenticated,
      json: async () => ({ isAuthenticated, role: isAuthenticated ? 'Buyer' : null }),
    } as Response);
  }

  const result = render(
    <MemoryRouter initialEntries={['/login']}>
      <AuthProvider>
        <PublicRoute>
          <TestChild />
        </PublicRoute>
      </AuthProvider>
    </MemoryRouter>
  );

  if (!isLoading) {
    await new Promise(resolve => setTimeout(resolve, 50));
  }

  return result;
};

describe('PublicRoute Component', () => {
  beforeEach(() => {
    mockApiClient.mockClear();
  });

  test('shows loading state while checking authentication', async () => {
    await renderPublicRoute(false, true);

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.queryByText('Public Content')).not.toBeInTheDocument();
  });

  test('renders children when not authenticated', async () => {
    await renderPublicRoute(false);

    expect(screen.getByText('Public Content')).toBeInTheDocument();
  });

  test('redirects to home when authenticated', async () => {
    await renderPublicRoute(true);

    // Content should not be visible when authenticated
    expect(screen.queryByText('Public Content')).not.toBeInTheDocument();
  });

  test('renders multiple children when not authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: false,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    render(
      <MemoryRouter initialEntries={['/login']}>
        <AuthProvider>
          <PublicRoute>
            <div>Child 1</div>
            <div>Child 2</div>
            <div>Child 3</div>
          </PublicRoute>
        </AuthProvider>
      </MemoryRouter>
    );

    await new Promise(resolve => setTimeout(resolve, 50));

    expect(screen.getByText('Child 1')).toBeInTheDocument();
    expect(screen.getByText('Child 2')).toBeInTheDocument();
    expect(screen.getByText('Child 3')).toBeInTheDocument();
  });

  test('preserves children props when not authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: false,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    const ChildWithProps = ({ message }: { message: string }) => (
      <div data-testid="child-with-props">{message}</div>
    );

    render(
      <MemoryRouter initialEntries={['/login']}>
        <AuthProvider>
          <PublicRoute>
            <ChildWithProps message="Login Page" />
          </PublicRoute>
        </AuthProvider>
      </MemoryRouter>
    );

    await new Promise(resolve => setTimeout(resolve, 50));

    expect(screen.getByTestId('child-with-props')).toHaveTextContent('Login Page');
  });
});
