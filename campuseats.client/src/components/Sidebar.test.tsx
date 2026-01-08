import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { MemoryRouter } from 'react-router-dom';
import Sidebar from './Sidebar';
import { AuthProvider } from '../context/AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

const renderSidebar = async (isAuthenticated: boolean, role: string | null = null) => {
  mockApiClient.mockResolvedValue({
    ok: isAuthenticated,
    json: async () => ({ isAuthenticated, role }),
  } as Response);

  const result = render(
    <MemoryRouter>
      <AuthProvider>
        <Sidebar />
      </AuthProvider>
    </MemoryRouter>
  );

  // Wait for auth check to complete
  await new Promise(resolve => setTimeout(resolve, 50));

  return result;
};

describe('Sidebar Component', () => {
  beforeEach(() => {
    mockApiClient.mockClear();
    mockNavigate.mockClear();
  });

  test('returns null when not authenticated', async () => {
    const { container } = await renderSidebar(false);
    expect(container.firstChild).toBeNull();
  });

  test('renders navigation links for authenticated user', async () => {
    await renderSidebar(true, 'Buyer');

    expect(screen.getByText('NAVIGARE')).toBeInTheDocument();
    expect(screen.getByText('Home')).toBeInTheDocument();
    expect(screen.getByText('Menu')).toBeInTheDocument();
  });

  test('shows "Comenzi" link for Buyer role', async () => {
    await renderSidebar(true, 'Buyer');

    expect(screen.getByText('Comenzi')).toBeInTheDocument();
  });

  test('shows "Comenzi" link for Admin role', async () => {
    await renderSidebar(true, 'Admin');

    expect(screen.getByText('Comenzi')).toBeInTheDocument();
  });

  test('does not show "Comenzi" link for Kitchen role', async () => {
    await renderSidebar(true, 'Kitchen');

    expect(screen.queryByText('Comenzi')).not.toBeInTheDocument();
  });

  test('shows admin section for Kitchen role', async () => {
    await renderSidebar(true, 'Kitchen');

    expect(screen.getByText('ADMINISTRARE')).toBeInTheDocument();
    expect(screen.getByText('Kitchen Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Adaugă produs')).toBeInTheDocument();
  });

  test('shows admin section for Admin role', async () => {
    await renderSidebar(true, 'Admin');

    expect(screen.getByText('ADMINISTRARE')).toBeInTheDocument();
    expect(screen.getByText('Kitchen Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Adaugă produs')).toBeInTheDocument();
  });

  test('does not show admin section for Buyer role', async () => {
    await renderSidebar(true, 'Buyer');

    expect(screen.queryByText('ADMINISTRARE')).not.toBeInTheDocument();
    expect(screen.queryByText('Kitchen Dashboard')).not.toBeInTheDocument();
  });

  test('shows Admin Dashboard link only for Admin role', async () => {
    await renderSidebar(true, 'Admin');

    expect(screen.getByText('Admin Dashboard')).toBeInTheDocument();
  });

  test('does not show Admin Dashboard link for Kitchen role', async () => {
    await renderSidebar(true, 'Kitchen');

    expect(screen.queryByText('Admin Dashboard')).not.toBeInTheDocument();
  });

  test('shows profile and logout for all authenticated users', async () => {
    await renderSidebar(true, 'Buyer');

    expect(screen.getByText('CONTUL MEU')).toBeInTheDocument();
    expect(screen.getByText('Profil')).toBeInTheDocument();
    expect(screen.getByText('Logout')).toBeInTheDocument();
  });

  test('logout button calls API', async () => {
    mockApiClient.mockResolvedValue({ ok: true } as Response);

    await renderSidebar(true, 'Buyer');

    const logoutButton = screen.getByText('Logout');
    await userEvent.click(logoutButton);

    // Wait for async operations
    await new Promise(resolve => setTimeout(resolve, 100));

    expect(mockApiClient).toHaveBeenCalledWith('/api/user/logout', { method: 'POST' });
  });

  test('logout handles API errors gracefully', async () => {
    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
    mockApiClient.mockRejectedValue(new Error('Logout failed'));

    await renderSidebar(true, 'Buyer');

    const logoutButton = screen.getByText('Logout');
    await userEvent.click(logoutButton);

    // Wait for async operations
    await new Promise(resolve => setTimeout(resolve, 100));

    expect(consoleErrorSpy).toHaveBeenCalledWith('Logout failed', expect.any(Error));
    
    consoleErrorSpy.mockRestore();
  });

  test('has correct styling classes', async () => {
    const { container } = await renderSidebar(true, 'Buyer');

    const aside = container.querySelector('aside');
    expect(aside).toHaveClass('w-64');
    expect(aside).toHaveClass('bg-gray-800');
    expect(aside).toHaveClass('text-white');
    expect(aside).toHaveClass('min-h-screen');
  });

  test('navigation links have correct paths', async () => {
    await renderSidebar(true, 'Buyer');

    const homeLink = screen.getByText('Home').closest('a');
    const menuLink = screen.getByText('Menu').closest('a');
    const ordersLink = screen.getByText('Comenzi').closest('a');
    const profileLink = screen.getByText('Profil').closest('a');

    expect(homeLink).toHaveAttribute('href', '/home');
    expect(menuLink).toHaveAttribute('href', '/menu');
    expect(ordersLink).toHaveAttribute('href', '/orders');
    expect(profileLink).toHaveAttribute('href', '/profile');
  });

  test('admin links have correct paths', async () => {
    await renderSidebar(true, 'Admin');

    const kitchenLink = screen.getByText('Kitchen Dashboard').closest('a');
    const addItemLink = screen.getByText('Adaugă produs').closest('a');
    const adminLink = screen.getByText('Admin Dashboard').closest('a');

    expect(kitchenLink).toHaveAttribute('href', '/kitchen-orders');
    expect(addItemLink).toHaveAttribute('href', '/add-menu-item');
    expect(adminLink).toHaveAttribute('href', '/admin');
  });
});
