import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
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
        fireEvent.change(screen.getByPlaceholderText(/username/i), { target: { value: 'testuser' } });
        fireEvent.change(screen.getByPlaceholderText(/parola/i), { target: { value: 'wrongpass' } });

        // Trimitem formularul
        fireEvent.click(screen.getByRole('button', { name: /intra in cont/i }));

        // FIX: Folosim findByText cu regex case-insensitive pentru flexibilitate maximă
        // findByText așteaptă automat să apară elementul (ca un waitFor)
        const errorMessage = await screen.findByText(/invalid credentials/i);
        expect(errorMessage).toBeInTheDocument();
    });

    it('should navigate to home on successful login', async () => {
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => ({}) } as Response);
        mockApiClient.mockResolvedValueOnce({ ok: true, json: async () => ({ isAuthenticated: true, role: 'Buyer' }) } as Response);

        render(
            <LanguageProvider>
                <AuthProvider>
                    <Login />
                </AuthProvider>
            </LanguageProvider>
        );

        fireEvent.change(screen.getByPlaceholderText(/username/i), { target: { value: 'admin' } });
        fireEvent.change(screen.getByPlaceholderText(/parola/i), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('button', { name: /intra in cont/i }));

        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/home');
        });
    });

    it('should handle network error during login', async () => {
        const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});
        mockApiClient.mockRejectedValue(new Error('Network error'));

        render(
            <LanguageProvider>
                <AuthProvider>
                    <Login />
                </AuthProvider>
            </LanguageProvider>
        );

        fireEvent.change(screen.getByPlaceholderText(/username/i), { target: { value: 'user' } });
        fireEvent.change(screen.getByPlaceholderText(/parola/i), { target: { value: 'pass' } });
        fireEvent.click(screen.getByRole('button', { name: /intra in cont/i }));

        const errorMsg = await screen.findByText('An error occurred. Please try again.');
        expect(errorMsg).toBeInTheDocument();

        expect(consoleSpy).toHaveBeenCalledWith('Error during login:', expect.any(Error));
        consoleSpy.mockRestore();
    });
});