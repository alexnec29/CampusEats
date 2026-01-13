import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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
        const user = userEvent.setup();
        // Simulăm un guest (fără rol)
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: false, role: null }) } as Response);
            return Promise.resolve({ ok: true, json: async () => mockMenuItems } as Response);
        });

        renderMenu();
        const addBtn = await screen.findByText('Adaugă în Coș');
        await user.click(addBtn);
        expect(mockNavigate).toHaveBeenCalledWith('/login');
    });

    it('should handle item deletion for Admin', async () => {
        const user = userEvent.setup();
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ isAuthenticated: true, role: 'Admin' }) } as Response);
            return Promise.resolve({ ok: true, json: async () => mockMenuItems } as Response);
        });

        renderMenu();
        const deleteBtns = await screen.findAllByTitle('Delete Item');
        await user.click(deleteBtns[0]);

        await user.click(screen.getByText('Șterge'));
        await waitFor(() => expect(screen.getByText(/Produs șters cu succes/i)).toBeInTheDocument());
    });
});