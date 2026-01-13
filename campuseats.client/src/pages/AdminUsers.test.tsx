import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import AdminUsers from './AdminUsers';
import { ToastProvider } from '../context/ToastContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const mockUsers = [
    { id: '1', username: 'john_doe', email: 'john@test.com', role: 'Buyer', loyaltyPoints: 100 },
    { id: '2', username: 'chef_mario', email: 'mario@kitchen.com', role: 'Kitchen' }
];

describe('AdminUsers Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should show loading spinner initially and then render users', async () => {
        mockApiClient.mockResolvedValueOnce({
            ok: true,
            json: async () => mockUsers,
        } as Response);

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        expect(screen.getByRole('status')).toBeInTheDocument();

        await waitFor(() => {
            expect(screen.queryByRole('status')).not.toBeInTheDocument();
        });

        expect(await screen.findByText('john_doe')).toBeInTheDocument();
        expect(screen.getByText('chef_mario')).toBeInTheDocument();
    });

    it('should handle role update', async () => {
        const user = userEvent.setup();
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        const select = await screen.findByDisplayValue('Kitchen');
        await user.selectOptions(select, 'Admin');

        expect(mockApiClient).toHaveBeenCalledWith(
            expect.stringContaining('/api/admin/users/2/role'),
            expect.objectContaining({ method: 'PUT' })
        );
    });

    it('should open loyalty modal and apply points adjustment', async () => {
        const user = userEvent.setup();
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);

        mockApiClient.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ pointsBalance: 150 }),
        } as Response);

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        const loyaltyBtn = await screen.findByRole('button', { name: /loyalty/i });
        await user.click(loyaltyBtn);

        expect(screen.getByText(/Loyalty – john_doe/i)).toBeInTheDocument();
        expect(screen.getByText('100')).toBeInTheDocument();

        const input = screen.getByPlaceholderText('+ / -');
        await user.type(input, '50');

        await user.click(screen.getByRole('button', { name: /apply/i }));

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith('/api/loyalty/adjust', expect.any(Object));
            expect(screen.getByText(/Puncte actualizate cu succes/i)).toBeInTheDocument();
        });
    });

    it('should show error toast when loyalty adjustment fails', async () => {
        const user = userEvent.setup();
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);
        mockApiClient.mockResolvedValueOnce({
            ok: false,
            text: async () => 'Error message from server',
        } as Response);

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        const loyaltyBtn = await screen.findByRole('button', { name: /loyalty/i });
        await user.click(loyaltyBtn);

        await user.type(screen.getByPlaceholderText('+ / -'), '10');
        await user.click(screen.getByRole('button', { name: /apply/i }));

        await waitFor(() => {
            expect(screen.getByText('Error message from server')).toBeInTheDocument();
        });
    });

    it('should close modal when cancel is clicked', async () => {
        const user = userEvent.setup();
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        const loyaltyBtn = await screen.findByRole('button', { name: /loyalty/i });
        await user.click(loyaltyBtn);

        expect(screen.getByText(/Loyalty – john_doe/i)).toBeInTheDocument();

        await user.click(screen.getByRole('button', { name: /cancel/i }));

        await waitFor(() => {
            expect(screen.queryByText(/Loyalty – john_doe/i)).not.toBeInTheDocument();
        });
    });
});