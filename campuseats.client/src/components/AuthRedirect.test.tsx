import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import '@testing-library/jest-dom';
import AuthRedirect from './AuthRedirect';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const ProtectedContent = () => <div>Protected Content</div>;
const RedirectPage = () => <div>Redirect Page</div>;

const renderWithRouter = (isAuthenticated: boolean, redirectTo: string, isLoading: boolean = false) => {
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
          <Route path={redirectTo} element={<RedirectPage />} />
          <Route
            path="/"
            element={
              <AuthRedirect redirectTo={redirectTo}>
                <ProtectedContent />
              </AuthRedirect>
            }
          />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('AuthRedirect', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should display loading state while checking authentication', () => {
    renderWithRouter(false, '/login', true);
    
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('should redirect to specified path when user is not authenticated', async () => {
    renderWithRouter(false, '/login');

    await screen.findByText('Redirect Page');
    expect(screen.getByText('Redirect Page')).toBeInTheDocument();
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
  });

  it('should render children when user is authenticated', async () => {
    renderWithRouter(true, '/login');

    await screen.findByText('Protected Content');
    expect(screen.getByText('Protected Content')).toBeInTheDocument();
    expect(screen.queryByText('Redirect Page')).not.toBeInTheDocument();
  });

  it('should redirect to custom path', async () => {
    renderWithRouter(false, '/custom-redirect');

    await screen.findByText('Redirect Page');
    expect(screen.getByText('Redirect Page')).toBeInTheDocument();
  });
});
