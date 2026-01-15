import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import KitchenOrders from './KitchenOrders';
import { AuthProvider } from '../context/AuthContext';
import { ToastProvider } from '../context/ToastContext';
import * as apiClientModule from '../utils/apiClient';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const mockOrders = [
    {
        id: 1,
        status: 2, // Paid
        orderDate: new Date().toISOString(),
        orderItems: [{ id: 10, quantity: 2, menuItem: { name: 'Burger' } }]
    }
];

describe('KitchenOrders Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();

        // Mock default pentru Auth (Kitchen role)
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ isAuthenticated: true, role: 'Kitchen' })
                } as Response);
            }
            // Mock pentru fetchOrders (cele 3 coloane)
            if (url.includes('/api/orders/status')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => url.includes('status=Paid') ? mockOrders : []
                } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => ({}) } as Response);
        });
    });

    const renderKitchen = () => render(
        <ToastProvider>
            <AuthProvider>
                <KitchenOrders />
            </AuthProvider>
        </ToastProvider>
    );

    it('should redirect non-kitchen users to home', async () => {
        mockApiClient.mockImplementationOnce(() => Promise.resolve({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Buyer' })
        } as Response));

        renderKitchen();

        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/');
        });
    });

    it('should display orders in the correct columns', async () => {
        renderKitchen();

        expect(await screen.findByText(/Incoming \(Paid\)/i)).toBeInTheDocument();
        expect(screen.getByText('#1')).toBeInTheDocument();
        expect(screen.getByText('2x Burger')).toBeInTheDocument();
    });

    it('should handle status update and move order between columns', async () => {
        renderKitchen();

        const startPreparingBtn = await screen.findByRole('button', { name: /start preparing/i });

        // Mock pentru succesul update-ului de status
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        fireEvent.click(startPreparingBtn);

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith(
                expect.stringContaining('/api/orders/1/status'),
                expect.objectContaining({ method: 'PUT' })
            );
            expect(screen.getByText(/Status actualizat/i)).toBeInTheDocument();
        });
    });

    it('should handle order cancellation', async () => {
        renderKitchen();

        const cancelBtn = await screen.findByRole('button', { name: /cancel/i });
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        fireEvent.click(cancelBtn);

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith(
                '/api/orders/cancel-by-kitchen',
                expect.objectContaining({ method: 'POST' })
            );
            expect(screen.getByText(/Comandă anulată cu succes/i)).toBeInTheDocument();
        });
    });
});