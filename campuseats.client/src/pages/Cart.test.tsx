import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import Cart from './Cart';
import { AuthProvider } from '../context/AuthContext';
import { ToastProvider } from '../context/ToastContext';
import { ConfirmProvider } from '../context/ConfirmContext';
import * as apiClientModule from '../utils/apiClient';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const mockCartData = [
    {
        id: 500,
        status: 0,
        totalAmount: 30.00,
        orderItems: [
            {
                id: 10,
                price: 15.00,
                quantity: 2,
                menuItem: { name: 'Burger Classic' }
            }
        ]
    }
];

describe('Cart Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();

        // Mock persistent care răspunde la ambele apeluri (Auth și Orders)
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ isAuthenticated: true, role: 'Buyer' })
                } as Response);
            }
            if (url.includes('/api/orders/my-orders')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => mockCartData
                } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => ({}) } as Response);
        });
    });

    const renderCart = () => render(
        <ToastProvider>
            <ConfirmProvider>
                <AuthProvider>
                    <Cart />
                </AuthProvider>
            </ConfirmProvider>
        </ToastProvider>
    );

    it('should render items when cart is not empty', async () => {
        renderCart();

        // Verificăm numele produsului
        expect(await screen.findByText('Burger Classic')).toBeInTheDocument();

        // FIX: Avem prețul de 2 ori (rând produs și total). Folosim getAllByText.
        const prices = screen.getAllByText('$30.00');
        expect(prices.length).toBeGreaterThanOrEqual(2);
    });

    it('should handle quantity increase', async () => {
        renderCart();

        // Căutăm butonul de plus după aria-label (mai robust decât textul "+")
        const increaseBtn = await screen.findByLabelText(/increase quantity/i);

        // Mock pentru succesul update-ului
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        fireEvent.click(increaseBtn);

        expect(mockApiClient).toHaveBeenCalledWith(
            expect.stringContaining('/api/orders/500/items/10'),
            expect.objectContaining({ method: 'PUT' })
        );
    });

    it('should remove item after confirmation', async () => {
        renderCart();

        const removeBtn = await screen.findByTitle('Remove item');

        // Resetăm mock-ul pentru DELETE
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        fireEvent.click(removeBtn);

        // Confirmăm în modala Context-ului
        const confirmBtn = screen.getByText('Șterge');
        fireEvent.click(confirmBtn);

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith(
                expect.stringContaining('/api/orders/500/items/10'),
                expect.objectContaining({ method: 'DELETE' })
            );
        });
    });

    it('should navigate to payment when order is placed', async () => {
        renderCart();

        const placeBtn = await screen.findByText('Plasează Comanda');
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        fireEvent.click(placeBtn);

        const finalConfirm = screen.getByText('Plasează');
        fireEvent.click(finalConfirm);

        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/payment', expect.any(Object));
        });
    });
});