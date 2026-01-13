import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import Register from './Register';
import { AuthProvider } from '../context/AuthContext';
import { LanguageProvider } from '../context/LanguageContext';
import * as apiClientModule from '../utils/apiClient';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('Register Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();

        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ isAuthenticated: false })
                } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => ({}) } as Response);
        });
    });

    const renderRegister = () => render(
        <LanguageProvider>
            <AuthProvider>
                <Register />
            </AuthProvider>
        </LanguageProvider>
    );

    it('should redirect to home if user is already authenticated', async () => {
        mockApiClient.mockImplementationOnce((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ isAuthenticated: true })
                } as Response);
            }
            return Promise.resolve({ ok: true } as Response);
        });

        renderRegister();

        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/home');
        });
    });

    it('should render all input fields and the submit button', async () => {
        renderRegister();

        expect(await screen.findByPlaceholderText(/alege un nume de utilizator/i)).toBeInTheDocument();
        expect(screen.getByPlaceholderText(/adresa-de-email@exemplu\.com/i)).toBeInTheDocument();
        expect(screen.getByPlaceholderText(/creeaza o parola/i)).toBeInTheDocument();
        expect(screen.getByPlaceholderText(/confirma parola/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /inregistreaza-te/i })).toBeInTheDocument();
    });

    it('should handle successful registration and navigate to login', async () => {
        const user = userEvent.setup();
        renderRegister();

        const usernameInput = await screen.findByPlaceholderText(/alege un nume de utilizator/i);
        const emailInput = screen.getByPlaceholderText(/adresa-de-email@exemplu\.com/i);
        const passwordInput = screen.getByPlaceholderText(/creeaza o parola/i);
        const confirmInput = screen.getByPlaceholderText(/confirma parola/i);
        const submitBtn = screen.getByRole('button', { name: /inregistreaza-te/i });

        await user.type(usernameInput, 'newuser');
        await user.type(emailInput, 'test@example.com');
        await user.type(passwordInput, 'password123');
        await user.type(confirmInput, 'password123');

        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        await user.click(submitBtn);

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith('/api/user/register', expect.objectContaining({
                method: 'POST',
                body: expect.stringContaining('"username":"newuser"')
            }));
            expect(mockNavigate).toHaveBeenCalledWith('/login');
        });
    });

    it('should display error message when registration fails', async () => {
        const user = userEvent.setup();
        renderRegister();

        const submitBtn = await screen.findByRole('button', { name: /inregistreaza-te/i });

        mockApiClient.mockResolvedValueOnce({
            ok: false,
            text: async () => 'Email already exists'
        } as Response);

        await user.click(submitBtn);

        expect(await screen.findByText(/Email already exists/i)).toBeInTheDocument();
    });

    it('should display generic error message on network failure', async () => {
        const user = userEvent.setup();
        renderRegister();

        const submitBtn = await screen.findByRole('button', { name: /inregistreaza-te/i });

        mockApiClient.mockRejectedValueOnce(new Error('Network Error'));

        await user.click(submitBtn);

        expect(await screen.findByText(/error occurred|eroare/i)).toBeInTheDocument();
    });
});