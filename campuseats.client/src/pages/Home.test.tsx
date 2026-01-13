import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import Home from './Home';
import { AuthProvider } from '../context/AuthContext';
import { LanguageProvider } from '../context/LanguageContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('react-router-dom', () => ({
    Link: ({ children, to }: any) => <a href={to}>{children}</a>,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('Home Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    const renderHome = () => render(
        <LanguageProvider>
            <AuthProvider>
                <Home />
            </AuthProvider>
        </LanguageProvider>
    );

    it('should show loading spinner initially', () => {
        mockApiClient.mockReturnValue(new Promise(() => {}));
        renderHome();
        expect(screen.getByRole('status')).toBeInTheDocument();
    });

    it('should show authentication required message when not logged in', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: false })
        } as Response);

        renderHome();

        // FIX: Folosim textul care apare real în debug-ul tău: "Trebuie sa te loghezi"
        const authMessage = await screen.findByText(/trebuie sa te loghezi/i);
        expect(authMessage).toBeInTheDocument();
        expect(screen.getByText(/intra in cont/i)).toBeInTheDocument();
    });

    it('should render welcome banner and quick actions when logged in', async () => {
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({
                        isAuthenticated: true,
                        username: 'Alex',
                        role: 'Buyer'
                    })
                } as Response);
            }
            return Promise.resolve({ ok: false } as Response);
        });

        renderHome();

        expect(await screen.findByText(/Alex/i)).toBeInTheDocument();
        expect(screen.getByText(/Bine ai venit/i)).toBeInTheDocument();

        // FIX: Pentru a evita "multiple elements", folosim getByRole('heading') pentru titluri
        expect(screen.getByRole('heading', { name: /meniu/i })).toBeInTheDocument();
        expect(screen.getByRole('heading', { name: /comenzile mele/i })).toBeInTheDocument();
        expect(screen.getByRole('heading', { name: /profil/i })).toBeInTheDocument();
    });

    it('should show error message if user data fails to load', async () => {
        mockApiClient.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ isAuthenticated: true })
        } as Response);

        mockApiClient.mockResolvedValueOnce({ ok: false } as Response);

        renderHome();

        // FIX: Folosim textul real care apare în componentă
        const errorMessage = await screen.findByText(/nu am putut incarca datele/i);
        expect(errorMessage).toBeInTheDocument();
    });
});