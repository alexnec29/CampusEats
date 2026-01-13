import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import Login from './Login';
import { AuthProvider } from '../context/AuthContext';
import { LanguageProvider } from '../context/LanguageContext';
import * as apiClientModule from '../utils/apiClient';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
    Link: ({ children, to }: any) => <a href={to}>{children}</a>,
    useLocation: () => ({ pathname: '/' }),
    useParams: () => ({}),
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('Login Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: false, role: null }),
        } as Response);
    });

    it('should render login form with translations', async () => {
        render(
            <LanguageProvider>
                <AuthProvider>
                    <Login />
                </AuthProvider>
            </LanguageProvider>
        );

        expect(screen.getByRole('heading', { level: 2 })).toBeInTheDocument();
        // Căutăm după "username" deoarece placeholder-ul tău este "Introdu username-ul"
        expect(screen.getByPlaceholderText(/username/i)).toBeInTheDocument();
        expect(screen.getByRole('button')).toBeInTheDocument();
    });

    it('should show error message on failed login', async () => {
        const user = userEvent.setup();

        // 1. Primul apel (check-auth la render) -> returnează ok (neautentificat)
        mockApiClient.mockResolvedValueOnce({
            ok: true,
            json: async () => ({ isAuthenticated: false }),
        } as Response);

        // 2. Al doilea apel (clicul pe buton) -> returnează eroare
        mockApiClient.mockResolvedValueOnce({
            ok: false,
            text: async () => 'Invalid credentials',
        } as Response);

        render(
            <LanguageProvider>
                <AuthProvider>
                    <Login />
                </AuthProvider>
            </LanguageProvider>
        );

        // Completăm câmpurile
        await user.type(screen.getByPlaceholderText(/username/i), 'testuser');
        await user.type(screen.getByPlaceholderText(/parola/i), 'wrongpass');

        // Trimitem formularul
        await user.click(screen.getByRole('button', { name: /intra in cont/i }));

        // FIX: Folosim findByText cu regex case-insensitive pentru flexibilitate maximă
        // findByText așteaptă automat să apară elementul (ca un waitFor)
        const errorMessage = await screen.findByText(/invalid credentials/i);
        expect(errorMessage).toBeInTheDocument();
    });

    it('should navigate to home on successful login', async () => {
        const user = userEvent.setup();
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => ({}) } as Response);
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response);

        render(
            <LanguageProvider>
                <AuthProvider>
                    <Login />
                </AuthProvider>
            </LanguageProvider>
        );

        await user.type(screen.getByPlaceholderText(/username/i), 'admin');
        await user.type(screen.getByPlaceholderText(/parola/i), 'password123');
        await user.click(screen.getByRole('button', { name: /intra in cont/i }));

        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/home');
        });
    });
});