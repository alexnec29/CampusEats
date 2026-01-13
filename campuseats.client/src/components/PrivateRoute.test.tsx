import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import '@testing-library/jest-dom';
import PrivateRoute from './PrivateRoute';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const ProtectedContent = () => <div>Protected Content</div>;
const LoginPage = () => <div>Login Page</div>;

const renderWithRouter = (isAuthenticated: boolean, isLoading: boolean = false) => {
  if (isLoading) {
    mockApiClient.mockImplementation(() => new Promise(() => {}));
  } else {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated, role: isAuthenticated ? 'Buyer' : null }),
    } as Response);
  }

  return render(
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/"
            element={
              <PrivateRoute>
                <ProtectedContent />
              </PrivateRoute>
            }
          />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('PrivateRoute', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should display loading state while checking authentication', () => {
    renderWithRouter(false, true);
    
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('should redirect to login when user is not authenticated', async () => {
    renderWithRouter(false);

    await screen.findByText('Login Page');
    expect(screen.getByText('Login Page')).toBeInTheDocument();
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('should render children when user is authenticated', async () => {
    renderWithRouter(true);

    await screen.findByText('Protected Content');
    expect(screen.getByText('Protected Content')).toBeInTheDocument();
    expect(screen.queryByText('Login Page')).not.toBeInTheDocument();
  });
});
