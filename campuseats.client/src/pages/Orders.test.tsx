import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import Orders from './Orders';
import { AuthProvider } from '../context/AuthContext';
import { ToastProvider } from '../context/ToastContext';
import { ConfirmProvider } from '../context/ConfirmContext';
import { OrderStatus } from '../types';
import * as apiClientModule from '../utils/apiClient';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const mockOrdersData = [
    {
        id: 1,
        status: 7,
        orderDate: '2025-01-10T10:00:00Z',
        totalAmount: 25.00,
        orderItems: [{ id: 10, price: 12.50, quantity: 2, menuItem: { name: 'Burger' } }]
    },
    {
        id: 2,
        status: 1,
        orderDate: '2025-01-11T10:00:00Z',
        totalAmount: 10.00,
        orderItems: []
    }
];

describe('Orders Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            }
            if (url.includes('/api/orders/my-orders')) {
                return Promise.resolve({ ok: true, json: async () => mockOrdersData } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => [] } as Response);
        });
    });

    const renderOrders = () => render(
        <ToastProvider>
            <ConfirmProvider>
                <AuthProvider>
                    <Orders />
                </AuthProvider>
            </ConfirmProvider>
        </ToastProvider>
    );

    it('should show loading spinner initially', () => {
        mockApiClient.mockReturnValueOnce(new Promise(() => {}));
        renderOrders();
        expect(screen.getByRole('status')).toBeInTheDocument();
    });

    it('should render orders list and filter out pending ones', async () => {
        renderOrders();

        expect(await screen.findByText(/Order #1/i)).toBeInTheDocument();

        expect(screen.getByText('Paid')).toBeInTheDocument();

        expect(screen.getByText((content) => content.includes('2') && content.includes('Burger'))).toBeInTheDocument();

        expect(screen.queryByText(/Order #2/i)).not.toBeInTheDocument();
    });

    it('should show empty message when no orders exist', async () => {
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            return Promise.resolve({ ok: true, json: async () => [] } as Response);
        });

        renderOrders();
        expect(await screen.findByText(/no orders found/i)).toBeInTheDocument();
    });
});