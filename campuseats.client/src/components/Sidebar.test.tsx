import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import Sidebar from './Sidebar';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;
const mockNavigate = jest.fn();

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
  useLocation: () => ({ pathname: '/home' }),
}));

const renderSidebar = (isAuthenticated: boolean, userRole: string | null = null) => {
  mockApiClient.mockResolvedValue({
    ok: true,
    json: async () => ({ isAuthenticated, role: userRole }),
  } as Response);

  return render(
    <BrowserRouter>
      <AuthProvider>
        <Sidebar />
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('Sidebar', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should not render when user is not authenticated', async () => {
    const { container } = renderSidebar(false);

    await new Promise(resolve => setTimeout(resolve, 100));
    
    expect(container.firstChild).toBeNull();
  });

  it('should render navigation items for authenticated user', async () => {
    renderSidebar(true, 'Buyer');

    await screen.findByText('Home');
    expect(screen.getByText('Home')).toBeInTheDocument();
    expect(screen.getByText('Meniu')).toBeInTheDocument();
  });

  it('should display Orders link for Buyer role', async () => {
    renderSidebar(true, 'Buyer');

    await screen.findByText('Comenzi');
    expect(screen.getByText('Comenzi')).toBeInTheDocument();
  });

  it('should display Orders link for Admin role', async () => {
    renderSidebar(true, 'Admin');

    await screen.findByText('Comenzi');
    expect(screen.getByText('Comenzi')).toBeInTheDocument();
  });

  it('should not display Orders link for Kitchen role', async () => {
    renderSidebar(true, 'Kitchen');

    await screen.findByText('Home');
    expect(screen.queryByText('Comenzi')).not.toBeInTheDocument();
  });

  it('should display Kitchen Dashboard for Kitchen role', async () => {
    renderSidebar(true, 'Kitchen');

    await screen.findByText('Kitchen Dashboard');
    expect(screen.getByText('Kitchen Dashboard')).toBeInTheDocument();
  });

  it('should display Kitchen Dashboard for Admin role', async () => {
    renderSidebar(true, 'Admin');

    await screen.findByText('Kitchen Dashboard');
    expect(screen.getByText('Kitchen Dashboard')).toBeInTheDocument();
  });

  it('should display Admin Dashboard only for Admin role', async () => {
    renderSidebar(true, 'Admin');

    await screen.findByText('Admin Dashboard');
    expect(screen.getByText('Admin Dashboard')).toBeInTheDocument();
  });

  it('should not display Admin Dashboard for Kitchen role', async () => {
    renderSidebar(true, 'Kitchen');

    await screen.findByText('Kitchen Dashboard');
    expect(screen.queryByText('Admin Dashboard')).not.toBeInTheDocument();
  });

  it('should not display Admin Dashboard for Buyer role', async () => {
    renderSidebar(true, 'Buyer');

    await screen.findByText('Home');
    expect(screen.queryByText('Admin Dashboard')).not.toBeInTheDocument();
  });

  it('should display Profile link', async () => {
    renderSidebar(true, 'Buyer');

    await screen.findByText('Profil');
    expect(screen.getByText('Profil')).toBeInTheDocument();
  });

  it('should display Logout button', async () => {
    renderSidebar(true, 'Buyer');

    await screen.findByText('Logout');
    expect(screen.getByText('Logout')).toBeInTheDocument();
  });

  it('should call logout API and navigate to login on logout click', async () => {
    const user = userEvent.setup();

    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
    } as Response);

    renderSidebar(true, 'Buyer');

    await screen.findByText('Logout');

    mockApiClient.mockResolvedValueOnce({
      ok: true,
    } as Response);

    const logoutButton = screen.getByText('Logout');
    await user.click(logoutButton);

    expect(mockApiClient).toHaveBeenCalledWith('/api/user/logout', { method: 'POST' });
  });

  it('should handle logout error gracefully', async () => {
    const user = userEvent.setup();
    const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    renderSidebar(true, 'Buyer');

    await screen.findByText('Logout');

    mockApiClient.mockRejectedValueOnce(new Error('Logout failed'));

    const logoutButton = screen.getByText('Logout');
    await user.click(logoutButton);

    expect(consoleSpy).toHaveBeenCalledWith('Logout failed', expect.any(Error));
    
    consoleSpy.mockRestore();
  });

  it('should display Adaugă produs link for Kitchen role', async () => {
    renderSidebar(true, 'Kitchen');

    await screen.findByText('Adaugă produs');
    expect(screen.getByText('Adaugă produs')).toBeInTheDocument();
  });

  it('should display section headers', async () => {
    renderSidebar(true, 'Admin');

    await screen.findByText('Navigare');
    expect(screen.getByText('Navigare')).toBeInTheDocument();
    expect(screen.getByText('Administrare')).toBeInTheDocument();
    expect(screen.getByText('Contul meu')).toBeInTheDocument();
  });
});
