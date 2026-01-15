import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
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
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        const select = await screen.findByDisplayValue('Kitchen');
        fireEvent.change(select, { target: { value: 'Admin' } });

        expect(mockApiClient).toHaveBeenCalledWith(
            expect.stringContaining('/api/admin/users/2/role'),
            expect.objectContaining({ method: 'PUT' })
        );
    });

    it('should open loyalty modal and apply points adjustment', async () => {
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
        fireEvent.click(loyaltyBtn);

        expect(screen.getByText(/Loyalty – john_doe/i)).toBeInTheDocument();
        expect(screen.getByText('100')).toBeInTheDocument();

        const input = screen.getByPlaceholderText('+ / -');
        fireEvent.change(input, { target: { value: '50' } });

        fireEvent.click(screen.getByRole('button', { name: /apply/i }));

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith('/api/loyalty/adjust', expect.any(Object));
            expect(screen.getByText(/Puncte actualizate cu succes/i)).toBeInTheDocument();
        });
    });

    it('should show error toast when loyalty adjustment fails', async () => {
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
        fireEvent.click(loyaltyBtn);

        fireEvent.change(screen.getByPlaceholderText('+ / -'), { target: { value: '10' } });
        fireEvent.click(screen.getByRole('button', { name: /apply/i }));

        await waitFor(() => {
            expect(screen.getByText('Error message from server')).toBeInTheDocument();
        });
    });

    it('should close modal when cancel is clicked', async () => {
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        const loyaltyBtn = await screen.findByRole('button', { name: /loyalty/i });
        fireEvent.click(loyaltyBtn);

        expect(screen.getByText(/Loyalty – john_doe/i)).toBeInTheDocument();

        fireEvent.click(screen.getByRole('button', { name: /cancel/i }));

        await waitFor(() => {
            expect(screen.queryByText(/Loyalty – john_doe/i)).not.toBeInTheDocument();
        });
    });

    it('should handle network error during loyalty adjustment', async () => {
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);
        
        const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
        mockApiClient.mockResolvedValueOnce({
           // fetch response? No, we want reject for catch block
        } as any);

        // Actually better to reject for catch block
        mockApiClient.mockRejectedValueOnce(new Error('Network error')); // This might reject the fetchUsers too if not sequenced right

        // Let's sequence properly.
        // 1. fetchUsers -> success
        // 2. applyAdjustment -> reject
    });

    it('should handle network error during loyalty adjustment (catch block)', async () => {
        // 1. fetchUsers ok
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response);
        // 2. applyAdjustment rejected
        mockApiClient.mockRejectedValueOnce(new Error('Network fail'));

        const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        const loyaltyBtn = await screen.findByRole('button', { name: /loyalty/i });
        fireEvent.click(loyaltyBtn);

        fireEvent.change(screen.getByPlaceholderText('+ / -'), { target: { value: '50' } });
        fireEvent.click(screen.getByRole('button', { name: /apply/i }));

        await waitFor(() => {
            expect(screen.getByText('Eroare la actualizarea punctelor')).toBeInTheDocument();
            expect(consoleSpy).toHaveBeenCalledWith('Error adjusting points', expect.any(Error));
        });
        consoleSpy.mockRestore();
    });

    it('should not apply adjustment if delta is 0', async () => {
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => mockUsers } as Response); // fetchUsers

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );
        
        await screen.findByText('john_doe'); // Wait for load

        const loyaltyBtn = screen.getByRole('button', { name: /loyalty/i });
        fireEvent.click(loyaltyBtn);

        // Delta is 0 by default. Click apply.
        fireEvent.click(screen.getByRole('button', { name: /apply/i }));

        // No new API calls (only the initial fetch)
        expect(mockApiClient).toHaveBeenCalledTimes(1); 
    });

    it('should handle error during users fetch', async () => {
        const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
        mockApiClient.mockRejectedValue(new Error('Fetch failed'));

        render(
            <ToastProvider>
                <AdminUsers />
            </ToastProvider>
        );

        await waitFor(() => {
            expect(consoleSpy).toHaveBeenCalledWith('Error loading users', expect.any(Error));
        });
        
        expect(screen.queryByRole('status')).not.toBeInTheDocument(); // Loading stops
        consoleSpy.mockRestore();
    });
});