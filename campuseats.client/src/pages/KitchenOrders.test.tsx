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

// --- Helper Data ---
const mockPaidOrder = {
    id: 101,
    status: 2, // Paid
    orderDate: new Date().toISOString(),
    orderItems: [{ id: 10, quantity: 2, menuItem: { name: 'Burger' } }]
};

const mockPreparingOrder = {
    id: 102,
    status: 3, // Preparing
    orderDate: new Date().toISOString(),
    orderItems: [{ id: 11, quantity: 1, menuItem: { name: 'Pizza' } }]
};

const mockReadyOrder = {
    id: 103,
    status: 4, // Ready
    orderDate: new Date().toISOString(),
    orderItems: [{ id: 12, quantity: 3, menuItem: { name: 'Salad' } }]
};

describe('KitchenOrders Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        // Default Auth Mock
        mockApiClient.mockImplementation(async (url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Kitchen' }) } as Response;
            }
            return { ok: true, json: async () => [] } as Response;
        });
    });

    const renderKitchen = () => render(
        <ToastProvider>
            <AuthProvider>
                <KitchenOrders />
            </AuthProvider>
        </ToastProvider>
    );

    it('should redirect non-kitchen users (e.g. Buyer) to home', async () => {
        mockApiClient.mockImplementation(async (url: string) => {
             if (url.includes('/api/user/check-auth')) {
                return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response;
            }
            return { ok: true, json: async () => [] } as Response;
        });

        renderKitchen();
        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/');
        });
    });

    it('should allow Admin access', async () => {
        mockApiClient.mockImplementation(async (url: string) => {
            if (url.includes('/api/user/check-auth')) {
               return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Admin' }) } as Response;
           }
           if (url.includes('/api/orders/status')) return { ok: true, json: async () => [] } as Response;
           return { ok: true } as Response;
       });

       renderKitchen();
       // Should NOT redirect
       await waitFor(() => expect(screen.getByText(/Kitchen Dashboard/i)).toBeInTheDocument());
       expect(mockNavigate).not.toHaveBeenCalled();
    });

    it('should display orders in the correct columns', async () => {
        mockApiClient.mockImplementation(async (url: string) => {
            if (url.includes('/api/user/check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Kitchen' }) } as Response;
            
            if (url.includes('status=Paid')) return { ok: true, json: async () => [mockPaidOrder] } as Response;
            if (url.includes('status=Preparing')) return { ok: true, json: async () => [mockPreparingOrder] } as Response;
            if (url.includes('status=Ready')) return { ok: true, json: async () => [mockReadyOrder] } as Response;

            return { ok: true } as Response;
        });

        renderKitchen();

        // Check Paid
        expect(await screen.findByText('#101')).toBeInTheDocument();
        expect(screen.getByText('2x Burger')).toBeInTheDocument();
        
        // Check Preparing
        expect(screen.getByText('#102')).toBeInTheDocument();
        expect(screen.getByText('1x Pizza')).toBeInTheDocument();

        // Check Ready
        expect(screen.getByText('#103')).toBeInTheDocument();
        expect(screen.getByText('3x Salad')).toBeInTheDocument();
    });

    it('should display empty state messages', async () => {
        // Default mocks return [] for lists
        renderKitchen();
        
        expect(await screen.findByText('No new orders')).toBeInTheDocument();
        expect(await screen.findByText('No orders in prep')).toBeInTheDocument();
        expect(await screen.findByText('No ready orders')).toBeInTheDocument();
    });

    it('should handle status update successfully (Paid -> Preparing)', async () => {
         mockApiClient.mockImplementation(async (url: string, opts?: any) => {
            if (url.includes('/api/user/check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Kitchen' }) } as Response;
            if (url.includes('status=Paid')) return { ok: true, json: async () => [mockPaidOrder] } as Response;
            
            if (url.includes('/api/orders/101/status') && opts?.method === 'PUT') {
                 return { ok: true } as Response; 
            }
            return { ok: true, json: async () => [] } as Response;
        });

        renderKitchen();
        const startBtn = await screen.findByRole('button', { name: /start preparing/i });
        fireEvent.click(startBtn);

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith(
                expect.stringContaining('/api/orders/101/status'),
                expect.objectContaining({ method: 'PUT', body: JSON.stringify({ status: 3 }) })
            );
            expect(screen.getByText(/Status actualizat la Preparing/i)).toBeInTheDocument();
        });
    });

    it('should show error when status update fails', async () => {
        mockApiClient.mockImplementation(async (url: string, opts?: any) => {
           if (url.includes('/api/user/check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Kitchen' }) } as Response;
           if (url.includes('status=Paid')) return { ok: true, json: async () => [mockPaidOrder] } as Response;
           
           if (url.includes('/api/orders/101/status') && opts?.method === 'PUT') {
                return { ok: false } as Response; 
           }
           return { ok: true, json: async () => [] } as Response;
       });

       renderKitchen();
       const startBtn = await screen.findByRole('button', { name: /start preparing/i });
       fireEvent.click(startBtn);

       await waitFor(() => {
           expect(screen.getByText(/Nu s-a putut actualiza statusul/i)).toBeInTheDocument();
       });
    });

    it('should handle order cancellation successfully', async () => {
        mockApiClient.mockImplementation(async (url: string, opts?: any) => {
            if (url.includes('/api/user/check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Kitchen' }) } as Response;
            if (url.includes('status=Paid')) return { ok: true, json: async () => [mockPaidOrder] } as Response;

            if (url.includes('/cancel-by-kitchen') && opts?.method === 'POST') {
                 return { ok: true } as Response; 
            }
            return { ok: true, json: async () => [] } as Response;
        });

        renderKitchen();
        const cancelBtn = await screen.findByRole('button', { name: /cancel/i });
        fireEvent.click(cancelBtn);

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith(
                '/api/orders/cancel-by-kitchen',
                expect.objectContaining({ method: 'POST', body: JSON.stringify({ orderId: 101 }) })
            );
            expect(screen.getByText(/Comandă anulată cu succes/i)).toBeInTheDocument();
        });
    });

    it('should handle cancellation error (already cancelled/conflict)', async () => {
        mockApiClient.mockImplementation(async (url: string, opts?: any) => {
            if (url.includes('/api/user/check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Kitchen' }) } as Response;
            if (url.includes('status=Paid')) return { ok: true, json: async () => [mockPaidOrder] } as Response;

            if (url.includes('/cancel-by-kitchen') && opts?.method === 'POST') {
                 return { ok: false, status: 400 } as Response; 
            }
            return { ok: true, json: async () => [] } as Response;
        });

        renderKitchen();
        const cancelBtn = await screen.findByRole('button', { name: /cancel/i });
        fireEvent.click(cancelBtn);

        await waitFor(() => {
            expect(screen.getByText(/Comanda este deja anulată/i)).toBeInTheDocument();
        });
    });

    it('should handle general exception during fetch', async () => {
         const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
         mockApiClient.mockImplementation(async (url: string) => {
            if (url.includes('/api/user/check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Kitchen' }) } as Response;
            throw new Error("Network Error");
        });

        renderKitchen();
        
        await waitFor(() => {
            expect(consoleSpy).toHaveBeenCalledWith('Error fetching kitchen orders:', expect.any(Error));
        });
        consoleSpy.mockRestore();
    });
});