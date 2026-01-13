import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import AdminDashboard from './AdminDashboard';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('AdminDashboard Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should show access denied for non-admin users', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
        } as Response);

        render(
            <AuthProvider>
                <AdminDashboard />
            </AuthProvider>
        );

        const accessDeniedMessage = await screen.findByText(/Access Denied/i);
        expect(accessDeniedMessage).toBeInTheDocument();
        expect(screen.queryByText(/Admin Dashboard/i)).not.toBeInTheDocument();
    });

    it('should render dashboard content for admin users', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Admin' }),
        } as Response);

        render(
            <AuthProvider>
                <AdminDashboard />
            </AuthProvider>
        );

        expect(await screen.findByText('Admin Dashboard')).toBeInTheDocument();
        expect(screen.getByText('User Management')).toBeInTheDocument();
        expect(screen.getByText('Menu Management')).toBeInTheDocument();
        expect(screen.getByText('Order Overview')).toBeInTheDocument();
    });

    it('should navigate to correct routes when management buttons are clicked', async () => {
        const user = userEvent.setup();
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Admin' }),
        } as Response);

        render(
            <AuthProvider>
                <AdminDashboard />
            </AuthProvider>
        );

        const manageUsersBtn = await screen.findByRole('button', { name: /Manage Users/i });
        await user.click(manageUsersBtn);
        expect(mockNavigate).toHaveBeenCalledWith('/admin/users');

        const manageMenuBtn = screen.getByRole('button', { name: /Manage Menu/i });
        await user.click(manageMenuBtn);
        expect(mockNavigate).toHaveBeenCalledWith('/menu');

        const viewOrdersBtn = screen.getByRole('button', { name: /View Orders/i });
        await user.click(viewOrdersBtn);
        expect(mockNavigate).toHaveBeenCalledWith('/orders');
    });
});