import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import Menu from './Menu';
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

const mockMenuItems = [
    { id: 1, name: 'Pizza', price: 20.0, description: 'Yummy', isAvailable: true, imageUrl: '' },
    { id: 2, name: 'Salad', price: 15.0, description: 'Healthy', isAvailable: false, imageUrl: '' }
];

describe('Menu Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response);
            }
            if (url.includes('/api/menu-items')) {
                return Promise.resolve({ ok: true, json: async () => mockMenuItems } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => [] } as Response);
        });
    });

    const renderMenu = () => render(
        <ToastProvider><ConfirmProvider><AuthProvider><Menu /></AuthProvider></ConfirmProvider></ToastProvider>
    );

    it('should show loading spinner initially', () => {
        mockApiClient.mockReturnValueOnce(new Promise(() => {}));
        renderMenu();
        expect(screen.getByRole('status')).toBeInTheDocument();
    });

    it('should render items correctly', async () => {
        renderMenu();
        expect(await screen.findByText('Pizza')).toBeInTheDocument();
        // Avem mai multe elemente cu Indisponibil, folosim getAll
        expect(screen.getAllByText(/indisponibil/i).length).toBeGreaterThan(0);
    });

    it('should redirect to login if unauthenticated user tries to add to cart', async () => {
        // Simulăm un guest (fără rol)
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: false, role: null }) } as Response);
            return Promise.resolve({ ok: true, json: async () => mockMenuItems } as Response);
        });

        renderMenu();
        const addBtn = await screen.findByText('Adaugă în Coș');
        fireEvent.click(addBtn);
        expect(mockNavigate).toHaveBeenCalledWith('/login');
    });

    it('should successfully add item to existing pending order', async () => {
        mockApiClient.mockImplementation(async (url: string, options?: any) => {
            if (url.includes('check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response;
            if (url.includes('menu-items') && (!options || options.method === 'GET')) return { ok: true, json: async () => mockMenuItems } as Response;
            
            // fetchMyOrders
            if (url.includes('my-orders')) return { ok: true, json: async () => [{ id: 101, status: 1 }] } as Response;
            
            // addItemToOrderRequest
            if (url.includes('/api/orders/101/items') && options?.method === 'POST') {
                return { ok: true } as Response;
            }
            return { ok: true, json: async () => [] } as Response;
        });

        renderMenu();
        const addBtns = await screen.findAllByText('Adaugă în Coș');
        fireEvent.click(addBtns[0]); // Add Pizza

        await waitFor(() => {
            expect(screen.getByText(/Pizza a fost adăugat în coș!/i)).toBeInTheDocument();
        });
    });

    it('should create new order if no pending order exists and add item', async () => {
        let myOrdersResponse: any[] = [];
        mockApiClient.mockImplementation(async (url: string, options?: any) => {
            if (url.includes('check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response;
            if (url.includes('menu-items')) return { ok: true, json: async () => mockMenuItems } as Response;
            if (url.includes('my-orders')) return { ok: true, json: async () => myOrdersResponse } as Response;
            
            // Create Order
            if (url === '/api/orders' && options?.method === 'POST') {
                myOrdersResponse = [{ id: 202, status: 1 }];
                return { ok: true, json: async () => ({ id: 202, status: 1 }) } as Response;
            }
            // Add Item
            if (url.includes('/api/orders/202/items') && options?.method === 'POST') {
                return { ok: true } as Response;
            }
            return { ok: true, json: async () => [] } as Response;
        });

        renderMenu();
        const addBtns = await screen.findAllByText('Adaugă în Coș');
        fireEvent.click(addBtns[0]); 

        await waitFor(() => {
             expect(mockApiClient).toHaveBeenCalledWith('/api/orders', expect.objectContaining({ method: 'POST' }));
             expect(screen.getByText(/Pizza a fost adăugat în coș!/i)).toBeInTheDocument();
        });
    });

    it('should handle conflict (409) when creating order by using existing orderId', async () => {
         mockApiClient.mockReset();
         mockApiClient.mockImplementation(async (url: string, options?: any) => {
             if (url.includes('check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response;
             if (url.includes('menu-items')) return { ok: true, json: async () => mockMenuItems } as Response;
             if (url.includes('my-orders')) return { ok: true, json: async () => [] } as Response;

             if (url === '/api/orders' && options?.method === 'POST') {
                 return { ok: false, status: 409, json: async () => ({ orderId: 303 }) } as Response;
             }
             if (url.includes('/api/orders/303/items')) return { ok: true } as Response;
             
             return { ok: true } as Response;
         });

         renderMenu();
         const addBtns = await screen.findAllByText('Adaugă în Coș');
         fireEvent.click(addBtns[0]); 

         await waitFor(() => {
             expect(mockApiClient).toHaveBeenCalledWith('/api/orders/303/items', expect.any(Object));
             expect(screen.getByText(/Pizza a fost adăugat în coș!/i)).toBeInTheDocument();
         });
    });

    it('should show error if adding item fails', async () => {
        mockApiClient.mockReset();
         mockApiClient.mockImplementation(async (url: string) => {
             if (url.includes('check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response;
             if (url.includes('menu-items')) return { ok: true, json: async () => mockMenuItems } as Response;
             if (url.includes('my-orders')) return { ok: true, json: async () => [{ id: 404, status: 1 }] } as Response;
             
             if (url.includes('/api/orders/404/items')) return { ok: false } as Response;
             return { ok: true } as Response;
         });

         renderMenu();
         const addBtns = await screen.findAllByText('Adaugă în Coș');
         fireEvent.click(addBtns[0]);

         await waitFor(() => {
             expect(screen.getByText(/Nu s-a putut adăuga produsul/i)).toBeInTheDocument();
         });
    });

    it('should handle item deletion for Admin', async () => {
        mockApiClient.mockImplementation(async (url: string) => {
            if (url.includes('check-auth')) return { ok: true, json: async () => ({ isAuthenticated: true, role: 'Admin' }) } as Response;
            if (url.includes('menu-items')) {
                 if (url.endsWith('/1')) return { ok: true } as Response; // DELETE success
                 return { ok: true, json: async () => mockMenuItems } as Response;
            }
            return { ok: true } as Response;
        });

        renderMenu();
        const deleteBtns = await screen.findAllByTitle('Delete Item');
        fireEvent.click(deleteBtns[0]);
        
        fireEvent.click(screen.getByText('Șterge'));
        await waitFor(() => expect(screen.getByText(/Produs șters cu succes/i)).toBeInTheDocument());
    });
});