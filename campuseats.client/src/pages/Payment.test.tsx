import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import PaymentPage from './Payment'; // Importăm fișierul tău original
import { ToastProvider } from '../context/ToastContext';
import * as apiClientModule from '../utils/apiClient';

// 1. Mock pentru React Router
const mockNavigate = jest.fn();
// Folosim un obiect global pentru locație ca să-l putem modifica în teste
let mockLocationState = { orderId: 123 };

jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
    useLocation: () => ({ state: mockLocationState }),
}));

// 2. Mock complet pentru Stripe (partea critică)
jest.mock('@stripe/stripe-js', () => ({
    loadStripe: jest.fn().mockResolvedValue({}),
}));

jest.mock('@stripe/react-stripe-js', () => ({
    Elements: ({ children }: any) => <div data-testid="stripe-elements">{children}</div>,
    PaymentElement: () => <div data-testid="mock-payment-element" />,
    useStripe: () => ({
        confirmPayment: jest.fn().mockResolvedValue({ error: null }),
    }),
    useElements: () => ({}),
}));

// 3. Mock pentru API
jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('Payment Page (Original Source)', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockLocationState = { orderId: 123 }; // Resetăm starea pentru fiecare test
    });

    const renderPayment = () => render(
        <ToastProvider>
            <PaymentPage />
        </ToastProvider>
    );

    it('should redirect to orders if no orderId is present in router state', async () => {
        mockLocationState = null as any; // Simulăm lipsa orderId
        renderPayment();

        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/orders');
        });
    });

    it('should show loading text while fetching client secret', () => {
        // API-ul nu răspunde imediat
        mockApiClient.mockReturnValue(new Promise(() => {}));

        renderPayment();

        // Căutăm textul de loading pe care îl ai în cod la linia 143
        expect(screen.getByText(/loading secure checkout/i)).toBeInTheDocument();
    });

    it('should fetch secret and render payment form', async () => {
        mockApiClient.mockResolvedValueOnce({
            ok: true,
            json: async () => 'pi_test_secret_123',
        } as Response);

        renderPayment();

        // Verificăm dacă API-ul a fost apelat cu ID-ul corect
        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith(
                '/api/payments/create-payment-intent/stripe',
                expect.objectContaining({
                    method: 'POST',
                    body: JSON.stringify({ orderId: 123 })
                })
            );
        });

        // Verificăm dacă s-a afișat formularul (prin mock-ul nostru)
        expect(await screen.findByTestId('mock-payment-element')).toBeInTheDocument();
        expect(screen.getByText(/Order ID: #123/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /pay now/i })).toBeInTheDocument();
    });

    it('should handle API errors gracefully', async () => {
        // Simulăm un eșec la server
        mockApiClient.mockResolvedValueOnce({ ok: false } as Response);

        renderPayment();

        // Verificăm mesajul de eroare din catch-ul tău de la linia 119
        const errorMsg = await screen.findByText(/could not load payment information/i);
        expect(errorMsg).toBeInTheDocument();
    });

    it('should navigate back to orders when Cancel is clicked', async () => {
        mockApiClient.mockResolvedValueOnce({
            ok: true,
            json: async () => 'secret',
        } as Response);

        renderPayment();

        const cancelBtn = await screen.findByRole('button', { name: /cancel/i });
        await userEvent.click(cancelBtn);

        expect(mockNavigate).toHaveBeenCalledWith('/orders');
    });
});