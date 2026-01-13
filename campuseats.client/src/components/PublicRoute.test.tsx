import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import PublicRoute from './PublicRoute';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('react-router-dom', () => ({
    Navigate: ({ to }: any) => <div data-testid="navigate">Redirecting to {to}</div>,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('PublicRoute', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should show loading state initially', () => {
        mockApiClient.mockReturnValue(new Promise(() => {}));

        render(
            <AuthProvider>
                <PublicRoute>
                    <div>Public Content</div>
                </PublicRoute>
            </AuthProvider>
        );

        expect(screen.getByText(/loading/i)).toBeInTheDocument();
    });

    it('should redirect to home if user is already authenticated', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
        } as Response);

        render(
            <AuthProvider>
                <PublicRoute>
                    <div>Public Content</div>
                </PublicRoute>
            </AuthProvider>
        );

        await waitFor(() => {
            expect(screen.getByTestId('navigate')).toHaveTextContent('Redirecting to /home');
        });
    });

    it('should render children if user is not authenticated', async () => {
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: false }),
        } as Response);

        render(
            <AuthProvider>
                <PublicRoute>
                    <div>Public Content</div>
                </PublicRoute>
            </AuthProvider>
        );

        const content = await screen.findByText('Public Content');
        expect(content).toBeInTheDocument();
    });
});