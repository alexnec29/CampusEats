import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import AuthRedirect from './AuthRedirect';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('react-router-dom', () => ({
    Navigate: ({ to }: any) => <div data-testid="navigate">Redirecting to {to}</div>,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('AuthRedirect', () => {
    it('should redirect to home if already authenticated', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
        } as Response);

        render(
            <AuthProvider>
                {/* FIX: Am adăugat prop-ul redirectTo="/home" */}
                <AuthRedirect redirectTo="/home">
                    <div>Login Page Content</div>
                </AuthRedirect>
            </AuthProvider>
        );

        await waitFor(() => {
            expect(screen.getByTestId('navigate')).toHaveTextContent('Redirecting to /home');
        });
    });

    it('should show content if not authenticated', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: false }),
        } as Response);

        render(
            <AuthProvider>
                {/* FIX: Am adăugat prop-ul redirectTo="/home" */}
                <AuthRedirect redirectTo="/home">
                    <div>Login Page Content</div>
                </AuthRedirect>
            </AuthProvider>
        );

        const content = await screen.findByText('Login Page Content');
        expect(content).toBeInTheDocument();
    });
});