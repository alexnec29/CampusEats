import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import PrivateRoute from './PrivateRoute';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

// Mock Router
jest.mock('react-router-dom', () => ({
    Navigate: ({ to }: any) => <div data-testid="navigate">Redirecting to {to}</div>,
}));

// Mock API
jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('PrivateRoute', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should show loading state', () => {
        // Simulăm un request care încă "stă"
        mockApiClient.mockReturnValue(new Promise(() => {}));

        render(
            <AuthProvider>
                <PrivateRoute><div>Protected Content</div></PrivateRoute>
            </AuthProvider>
        );

        expect(screen.getByText(/loading/i)).toBeInTheDocument();
    });

    it('should redirect to login if not authenticated', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: false }),
        } as Response);

        render(
            <AuthProvider>
                <PrivateRoute><div>Protected Content</div></PrivateRoute>
            </AuthProvider>
        );

        await waitFor(() => {
            expect(screen.getByTestId('navigate')).toHaveTextContent('Redirecting to /login');
        });
    });

    it('should render children if authenticated', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
        } as Response);

        render(
            <AuthProvider>
                <PrivateRoute><div>Protected Content</div></PrivateRoute>
            </AuthProvider>
        );

        const content = await screen.findByText('Protected Content');
        expect(content).toBeInTheDocument();
    });
});