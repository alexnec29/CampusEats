import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import Sidebar from './Sidebar';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

// 1. Mock total pentru react-router-dom (evităm eroarea de ESM)
const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
    useLocation: () => ({ pathname: '/home' }),
    Link: ({ children, to }: any) => <a href={to}>{children}</a>,
}));

// 2. Mock apiClient
jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('Sidebar Component', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should not render anything if not authenticated', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: false }),
        } as Response);

        render(
            <AuthProvider>
                <Sidebar />
            </AuthProvider>
        );

        // Sidebar returnează null dacă nu ești logat
        await waitFor(() => {
            expect(screen.queryByRole('complementary')).not.toBeInTheDocument();
        });
    });

    it('should render basic links for Buyer role', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
        } as Response);

        render(
            <AuthProvider>
                <Sidebar />
            </AuthProvider>
        );

        // Așteptăm să apară elementele de bază
        expect(await screen.findByText('Home')).toBeInTheDocument();
        expect(screen.getByText('Meniu')).toBeInTheDocument();
        expect(screen.getByText('Comenzi')).toBeInTheDocument();
        expect(screen.getByText('Profil')).toBeInTheDocument();

        // Nu ar trebui să vadă link-uri de Kitchen sau Admin
        expect(screen.queryByText('Kitchen Dashboard')).not.toBeInTheDocument();
        expect(screen.queryByText('Admin Dashboard')).not.toBeInTheDocument();
    });

    it('should render kitchen links for Kitchen role', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Kitchen' }),
        } as Response);

        render(
            <AuthProvider>
                <Sidebar />
            </AuthProvider>
        );

        expect(await screen.findByText('Kitchen Dashboard')).toBeInTheDocument();
        expect(screen.getByText('Adaugă produs')).toBeInTheDocument();

        // Kitchen nu ar trebui să vadă link-ul de Comenzi (Buyer) sau Admin Dashboard
        expect(screen.queryByText('Comenzi')).not.toBeInTheDocument();
        expect(screen.queryByText('Admin Dashboard')).not.toBeInTheDocument();
    });

    it('should render all links for Admin role', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Admin' }),
        } as Response);

        render(
            <AuthProvider>
                <Sidebar />
            </AuthProvider>
        );

        expect(await screen.findByText('Admin Dashboard')).toBeInTheDocument();
        expect(screen.getByText('Kitchen Dashboard')).toBeInTheDocument();
        expect(screen.getByText('Comenzi')).toBeInTheDocument();
    });

    it('should handle logout correctly', async () => {
        const user = userEvent.setup();
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
        } as Response);

        render(
            <AuthProvider>
                <Sidebar />
            </AuthProvider>
        );

        const logoutButton = await screen.findByText('Logout');

        // Mock pentru apelul de logout
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);
        // Mock pentru checkAuthStatus de după logout (care va returna false acum)
        mockApiClient.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ isAuthenticated: false }),
        } as Response);

        await user.click(logoutButton);

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith('/api/user/logout', { method: 'POST' });
            expect(mockNavigate).toHaveBeenCalledWith('/login');
        });
    });
});