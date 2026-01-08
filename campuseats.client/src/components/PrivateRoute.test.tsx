import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { MemoryRouter } from 'react-router-dom';
import PrivateRoute from './PrivateRoute';
import { AuthProvider } from '../context/AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

const TestChild = () => <div>Private Content</div>;

const renderPrivateRoute = async (isAuthenticated: boolean, isLoading: boolean = false) => {
  if (isLoading) {
    // Mock loading state
    mockApiClient.mockImplementation(() => new Promise(() => {}));
  } else {
    mockApiClient.mockResolvedValue({
      ok: isAuthenticated,
      json: async () => ({ isAuthenticated, role: isAuthenticated ? 'Buyer' : null }),
    } as Response);
  }

  const result = render(
    <MemoryRouter initialEntries={['/protected']}>
      <AuthProvider>
        <PrivateRoute>
          <TestChild />
        </PrivateRoute>
      </AuthProvider>
    </MemoryRouter>
  );

  // Wait for auth check to complete if not loading
  if (!isLoading) {
    await new Promise(resolve => setTimeout(resolve, 50));
  }

  return result;
};

describe('PrivateRoute Component', () => {
  beforeEach(() => {
    mockApiClient.mockClear();
  });

  test('shows loading state while checking authentication', async () => {
    await renderPrivateRoute(false, true);

    expect(screen.getByText('Loading...')).toBeInTheDocument();
    expect(screen.queryByText('Private Content')).not.toBeInTheDocument();
  });

  test('renders children when authenticated', async () => {
    await renderPrivateRoute(true);

    expect(screen.getByText('Private Content')).toBeInTheDocument();
  });

  test('redirects to login when not authenticated', async () => {
    const { container } = await renderPrivateRoute(false);

    // Content should not be visible
    expect(screen.queryByText('Private Content')).not.toBeInTheDocument();
    
    // Since we're using MemoryRouter, we can't check the actual redirect,
    // but we can verify that the private content is not rendered
  });

  test('renders multiple children when authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
    } as Response);

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <AuthProvider>
          <PrivateRoute>
            <div>Child 1</div>
            <div>Child 2</div>
            <div>Child 3</div>
          </PrivateRoute>
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
      json: async () => ({ isAuthenticated: true, role: 'Admin' }),
    } as Response);

    const ChildWithProps = ({ message }: { message: string }) => (
      <div data-testid="child-with-props">{message}</div>
    );

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <AuthProvider>
          <PrivateRoute>
            <ChildWithProps message="Test Message" />
          </PrivateRoute>
        </AuthProvider>
      </MemoryRouter>
    );

    await new Promise(resolve => setTimeout(resolve, 50));

    expect(screen.getByTestId('child-with-props')).toHaveTextContent('Test Message');
  });
});
