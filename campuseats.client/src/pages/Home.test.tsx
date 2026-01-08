import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { MemoryRouter } from 'react-router-dom';
import Home from './Home';
import { AuthProvider } from '../context/AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

const renderHome = async (isAuthenticated: boolean = true, userData: any = null) => {
  let callCount = 0;
  mockApiClient.mockImplementation(async (url: string) => {
    callCount++;
    if (callCount === 1 || !isAuthenticated) {
      return {
        ok: isAuthenticated,
        json: async () => ({ isAuthenticated, role: isAuthenticated ? 'Buyer' : null }),
      } as Response;
    }
    
    if (userData) {
      return {
        ok: true,
        json: async () => userData,
      } as Response;
    }
    
    return {
      ok: false,
      json: async () => ({}),
    } as Response;
  });

  const result = render(
    <MemoryRouter>
      <AuthProvider>
        <Home />
      </AuthProvider>
    </MemoryRouter>
  );

  await new Promise(resolve => setTimeout(resolve, 100));

  return result;
};

describe('Home Page', () => {
  beforeEach(() => {
    mockApiClient.mockClear();
  });

  test('renders home page for authenticated user', async () => {
    await renderHome(true, { username: 'testuser', role: 'Buyer' });

    await waitFor(() => {
      expect(screen.getByText(/testuser/i)).toBeInTheDocument();
    });
  });

  test('shows loading state initially', async () => {
    mockApiClient.mockImplementation(() => new Promise(() => {}));

    render(
      <BrowserRouter>
        <AuthProvider>
          <Home />
        </AuthProvider>
      </BrowserRouter>
    );

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  test('displays user role when available', async () => {
    await renderHome(true, { username: 'testuser', role: 'Buyer' });

    await waitFor(() => {
      const elements = screen.queryAllByText(/buyer/i);
      expect(elements.length).toBeGreaterThan(0);
    });
  });

  test('displays navigation links for authenticated users', async () => {
    await renderHome(true, { username: 'testuser', role: 'Buyer' });

    await waitFor(() => {
      expect(screen.getByText(/testuser/i)).toBeInTheDocument();
    });

    // Check for common navigation elements
    const menuLinks = screen.queryAllByText(/menu/i);
    expect(menuLinks.length).toBeGreaterThan(0);
  });

  test('handles user fetch failure gracefully', async () => {
    let callCount = 0;
    mockApiClient.mockImplementation(async () => {
      callCount++;
      if (callCount === 1) {
        return {
          ok: true,
          json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
        } as Response;
      }
      return {
        ok: false,
        json: async () => ({}),
      } as Response;
    });

    const { container } = render(
      <BrowserRouter>
        <AuthProvider>
          <Home />
        </AuthProvider>
      </BrowserRouter>
    );

    await new Promise(resolve => setTimeout(resolve, 100));

    // Should not crash when user fetch fails
    expect(container).toBeInTheDocument();
  });

  test('displays different content based on user role', async () => {
    const adminData = { username: 'admin', role: 'Admin', loyaltyPoints: 100 };
    await renderHome(true, adminData);

    await waitFor(() => {
      expect(screen.getByText(/admin/i)).toBeInTheDocument();
    });
  });

  test('renders correctly for Kitchen role', async () => {
    const kitchenData = { username: 'chef', role: 'Kitchen' };
    await renderHome(true, kitchenData);

    await waitFor(() => {
      expect(screen.getByText(/chef/i)).toBeInTheDocument();
    });
  });
});
