import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import Orders from './Orders';
import { OrderStatus } from '../types';
import { AuthProvider } from '../context/AuthContext';
import { ToastProvider } from '../context/ToastContext';
import * as apiClientModule from '../utils/apiClient';

// Mock mocks
const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
    Link: ({ children, to }: any) => <a href={to}>{children}</a>,
    useLocation: () => ({ pathname: '/orders' }),
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

// Mock Contexts
jest.mock('../context/ConfirmContext', () => ({
    useConfirm: () => ({
        confirm: jest.fn().mockResolvedValue(true)
    })
}));

// Mock window.confirm
const mockConfirm = jest.fn();
window.confirm = mockConfirm;

const mockOrders = [
    {
        id: 1,
        totalAmount: 45.0,
        status: OrderStatus.Placed,
        orderDate: '2023-11-20T10:00:00.000Z',
        orderItems: [
            { id: 101, menuItemId: 5, quantity: 2, price: 15.0, menuItem: { name: 'Pizza' } },
            { id: 102, menuItemId: 6, quantity: 1, price: 15.0, menuItem: { name: 'Pasta' } }
        ]
    },
    {
        id: 2,
        totalAmount: 12.5,
        status: OrderStatus.Completed,
        orderDate: '2023-11-19T14:30:00.000Z',
        orderItems: [
            { id: 103, menuItemId: 7, quantity: 1, price: 12.5, menuItem: { name: 'Salad' } }
        ]
    }
];

describe('Orders Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('renders loading state initially', async () => {
        mockApiClient.mockImplementation(() => new Promise(() => {})); // Never resolves
        render(
            <AuthProvider>
                <ToastProvider>
                    <Orders />
                </ToastProvider>
            </AuthProvider>
        );
        expect(screen.getByText(/Loading orders.../i)).toBeInTheDocument();
    });

    it('renders orders after fetch', async () => {
        mockApiClient.mockImplementation((url: RequestInfo | URL) => {
            if (url.toString().includes('check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ isAuthenticated: true, role: 'Buyer' })
                } as Response);
            }
            if (url.toString().includes('my-orders')) {
                return Promise.resolve({
                     ok: true,
                     json: async () => mockOrders
                } as Response);
            }
            return Promise.reject(new Error('Unknown URL'));
        });

        render(
            <AuthProvider>
                <ToastProvider>
                    <Orders />
                </ToastProvider>
            </AuthProvider>
        );

        await waitFor(() => {
            expect(screen.getByText('My Orders')).toBeInTheDocument();
        });

        expect(screen.getByText('Order #1')).toBeInTheDocument();
        expect(screen.getByText('2x Pizza')).toBeInTheDocument();
        expect(screen.getByText('Placed')).toBeInTheDocument();
        expect(screen.getByText('Completed')).toBeInTheDocument();
    });

    it('handles fetch error gracefully', async () => {
        const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
        
        mockApiClient.mockImplementation((url: RequestInfo | URL) => {
            if (url.toString().includes('check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ isAuthenticated: true })
                } as Response);
            }
            if (url.toString().includes('my-orders')) {
                return Promise.reject(new Error('Network error'));
            }
            return Promise.resolve({ ok: true } as Response);
        });

        render(
            <AuthProvider>
                <ToastProvider>
                    <Orders />
                </ToastProvider>
            </AuthProvider>
        );

        await waitFor(() => {
            expect(consoleSpy).toHaveBeenCalledWith('Error fetching orders:', expect.any(Error));
        });
        
        // Should stop loading even on error
        expect(screen.queryByText(/Loading orders.../i)).not.toBeInTheDocument();
        
        consoleSpy.mockRestore();
    });

    it('filters out Pending (Cart) orders', async () => {
        const ordersWithPending = [
            ...mockOrders,
            {
                id: 3,
                totalAmount: 0,
                status: OrderStatus.Pending, // Should be filtered
                orderDate: new Date().toISOString(),
                orderItems: []
            }
        ];

        mockApiClient.mockImplementation((url: RequestInfo | URL) => {
            if (url.toString().includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            if (url.toString().includes('my-orders')) return Promise.resolve({ ok: true, json: async () => ordersWithPending } as Response);
            return Promise.resolve({ ok: true } as Response);
        });

        render(
            <AuthProvider>
                <ToastProvider>
                    <Orders />
                </ToastProvider>
            </AuthProvider>
        );

        await waitFor(() => expect(screen.getByText('Order #1')).toBeInTheDocument());
        
        expect(screen.queryByText('Order #3')).not.toBeInTheDocument();
    });

    it('displays correct labels and colors for all statuses', async () => {
        const statuses = [
            { status: OrderStatus.Inactive, label: 'Inactive', colorClass: 'bg-gray-200' },
            { status: OrderStatus.Placed, label: 'Placed', colorClass: 'bg-blue-100' },
            { status: OrderStatus.PendingPayment, label: 'PendingPayment', colorClass: 'bg-gray-100' }, // Default color
            { status: OrderStatus.FailedPayment, label: 'FailedPayment', colorClass: 'bg-gray-100' },   // Default color
            { status: OrderStatus.Ready, label: 'Ready', colorClass: 'bg-green-100' },
            { status: OrderStatus.Completed, label: 'Completed', colorClass: 'bg-gray-100' },
            { status: OrderStatus.Cancelled, label: 'Cancelled', colorClass: 'bg-red-100' }
        ];

        // We create an order for each status (except Pending which is filtered)
        const allStatusOrders = statuses
            .filter(s => s.status !== OrderStatus.Pending)
            .map((s, idx) => ({
                id: 100 + idx,
                totalAmount: 10,
                status: s.status,
                orderDate: new Date().toISOString(),
                orderItems: []
            }));

        mockApiClient.mockImplementation((url: RequestInfo | URL) => {
            if (url.toString().includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true }) } as Response);
            if (url.toString().includes('my-orders')) return Promise.resolve({ ok: true, json: async () => allStatusOrders } as Response);
            return Promise.resolve({ ok: true } as Response);
        });

        render(
            <AuthProvider>
                <ToastProvider>
                    <Orders />
                </ToastProvider>
            </AuthProvider>
        );

        await waitFor(() => expect(screen.queryByText(/Loading orders.../i)).not.toBeInTheDocument());

        for (const s of statuses) {
             if (s.status === OrderStatus.Pending) continue;

             const labelElements = screen.getAllByText(s.label);
             // Might match multiple if duplicates, but here we have distinct labels mostly.
             
             const labelElement = labelElements[0];
             expect(labelElement).toBeInTheDocument();
             
             // Check color class check on the parent span
             // The structure is <span className={`... ${colorClass}`}>
             expect(labelElement).toHaveClass(s.colorClass!);
        }
    });
});
