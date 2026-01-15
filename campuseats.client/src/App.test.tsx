import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import App from './App';
import * as apiClientModule from './utils/apiClient';

// Mock API Client to prevent real calls
jest.mock('./utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

// Mock pages to simplify testing
jest.mock('./pages/Landing', () => () => <div data-testid="landing-page">Landing Page</div>);
jest.mock('./pages/Home', () => () => <div data-testid="home-page">Home Page</div>);
jest.mock('./pages/Login', () => () => <div data-testid="login-page">Login Page</div>);
jest.mock('./pages/Register', () => () => <div data-testid="register-page">Register Page</div>);
jest.mock('./pages/Profile', () => () => <div data-testid="profile-page">Profile Page</div>);
jest.mock('./pages/Menu', () => () => <div data-testid="menu-page">Menu Page</div>);
jest.mock('./pages/Orders', () => () => <div data-testid="orders-page">Orders Page</div>);
jest.mock('./pages/Cart', () => () => <div data-testid="cart-page">Cart Page</div>);
jest.mock('./pages/Payment', () => () => <div data-testid="payment-page">Payment Page</div>);
jest.mock('./pages/KitchenOrders', () => () => <div data-testid="kitchen-orders-page">Kitchen Orders Page</div>);
jest.mock('./pages/AddMenuItem', () => () => <div data-testid="add-menu-item-page">Add Menu Item Page</div>);
jest.mock('./pages/AdminDashboard', () => () => <div data-testid="admin-dashboard-page">Admin Dashboard</div>);
jest.mock('./pages/AdminUsers', () => () => <div data-testid="admin-users-page">Admin Users</div>);

// Mock Layout
jest.mock('./components/Layout', () => ({ children }: { children: React.ReactNode }) => <div data-testid="layout">{children}</div>);

describe('App Component', () => {
    beforeEach(() => {
        jest.clearAllMocks();
        // Default auth check response (not authenticated)
        mockApiClient.mockResolvedValue({
            ok: true,
            json: async () => ({ isAuthenticated: false, role: null }),
        } as Response);
    });

    it('renders landing page by default', async () => {
        // We need to ensure we are at root
        window.history.pushState({}, 'Test page', '/');
        
        render(<App />);

        await waitFor(() => {
            expect(screen.getByTestId('landing-page')).toBeInTheDocument();
        });
    });
});
