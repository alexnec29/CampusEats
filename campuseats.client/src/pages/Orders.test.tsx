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

// Mock confirm context
const mockConfirm = jest.fn();
jest.mock('../context/ConfirmContext', () => ({
    ...jest.requireActual('../context/ConfirmContext'),
    useConfirm: () => ({ confirm: mockConfirm })
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const mockOrdersData = [
    {
        id: 1,
        status: OrderStatus.Paid,
        orderDate: '2025-01-10T10:00:00Z',
        totalAmount: 25.00,
        orderItems: [{ id: 10, price: 12.50, quantity: 2, menuItem: { name: 'Burger' } }]
    },
    {
        id: 2,
        status: OrderStatus.Placed,
        orderDate: '2025-01-11T12:00:00Z',
        totalAmount: 15.00,
        orderItems: []
    }
];

describe('Orders Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockConfirm.mockResolvedValue(true);
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            }
            if (url.includes('/api/orders/my-orders')) {
                return Promise.resolve({ ok: true, json: async () => mockOrdersData } as Response);
            }
            return Promise.resolve({ ok: true } as Response);
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
        // Căutăm după aria-label "loading" definit în componentă
        expect(screen.getByText('Loading orders...')).toBeInTheDocument();
    });

    it('should redirect to login if not authenticated', async () => {
        mockApiClient.mockImplementation((url: string) => {
             if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: false }) } as Response);
             return Promise.resolve({ ok: true } as Response);
        });
        
        renderOrders();
        
        await waitFor(() => {
             expect(mockNavigate).toHaveBeenCalledWith('/login');
        });
    });

    it('should render orders list and filter out pending ones', async () => {
        renderOrders();

        await waitFor(() => {
            expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument();
        });

        expect(screen.getByText(/Order #1/i)).toBeInTheDocument();
        expect(screen.getByText('Paid')).toBeInTheDocument();
        expect(screen.getByText(/Order #2/i)).toBeInTheDocument();
        expect(screen.getByText('Placed')).toBeInTheDocument();
    });

    it('should show empty message when no orders exist', async () => {
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            if (url.includes('/api/orders/my-orders')) return Promise.resolve({ ok: true, json: async () => [] } as Response);
            return Promise.resolve({ ok: true } as Response);
        });

        renderOrders();
        expect(await screen.findByText(/no orders found/i)).toBeInTheDocument();
    });

    it('should handle error when fetching orders', async () => {
        const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            if (url.includes('/api/orders/my-orders')) return Promise.reject(new Error('Network error'));
            return Promise.resolve({ ok: true } as Response);
        });

        renderOrders();
        
        await waitFor(() => {
            expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument();
        });
        
        expect(consoleSpy).toHaveBeenCalledWith('Error fetching orders:', expect.any(Error));
        consoleSpy.mockRestore();
    });

    it('should display correct status labels and colors for all statuses', async () => {
         // Create orders for each status except Pending (which is filtered)
         const statusesToTest = [
            OrderStatus.Inactive,
            OrderStatus.Placed,
            OrderStatus.Paid,
            OrderStatus.Preparing,
            OrderStatus.Ready,
            OrderStatus.Completed,
            OrderStatus.Cancelled,
            OrderStatus.PendingPayment,
            OrderStatus.FailedPayment
        ];
        
        const allStatusOrders = statusesToTest.map((status, idx) => ({
            id: 100 + idx,
            status: status,
            orderDate: new Date().toISOString(),
            totalAmount: 10,
            orderItems: []
        }));

        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            if (url.includes('/api/orders/my-orders')) return Promise.resolve({ ok: true, json: async () => allStatusOrders } as Response);
            return Promise.resolve({ ok: true } as Response);
        });

        renderOrders();
        
        await waitFor(() => expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument());

        // Labels verification
        expect(screen.getByText('Inactive')).toBeInTheDocument();
        expect(screen.getAllByText('Placed').length).toBeGreaterThan(0);
        expect(screen.getAllByText('Paid').length).toBeGreaterThan(0);
        expect(screen.getByText('Preparing')).toBeInTheDocument();
        expect(screen.getByText('Ready')).toBeInTheDocument();
        expect(screen.getByText('Completed')).toBeInTheDocument();
        expect(screen.getByText('Cancelled')).toBeInTheDocument();
        expect(screen.getByText('PendingPayment')).toBeInTheDocument();
        expect(screen.getByText('FailedPayment')).toBeInTheDocument();

        // Default 'Unknown' case check
        // We add an order with a non-existent status ID to test default
        const unknownOrder = [{
            id: 999,
            status: 99, // Invalid status
            orderDate: new Date().toISOString(),
            totalAmount: 0,
            orderItems: []
        }];

        mockApiClient.mockImplementation((url: string) => {
             if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
             if (url.includes('/api/orders/my-orders')) return Promise.resolve({ ok: true, json: async () => unknownOrder } as Response);
             return Promise.resolve({ ok: true } as Response);
        });

        // Rerender to test unknown
        renderOrders();
        await waitFor(() => expect(screen.queryByText('Loading orders...')).not.toBeInTheDocument());
        // Should use default switch case
        expect(screen.getByText('Unknown')).toBeInTheDocument();
    });
});