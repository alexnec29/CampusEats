import React from 'react';
import { render, screen } from '@testing-library/react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import '@testing-library/jest-dom';
import PublicRoute from './PublicRoute';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const PublicContent = () => <div>Public Content</div>;
const HomePage = () => <div>Home Page</div>;

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
          <Route path="/home" element={<HomePage />} />
          <Route
            path="/"
            element={
              <PublicRoute>
                <PublicContent />
              </PublicRoute>
            }
          />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('PublicRoute', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should display loading state while checking authentication', () => {
    renderWithRouter(false, true);
    
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('should render children when user is not authenticated', async () => {
    renderWithRouter(false);

    await screen.findByText('Public Content');
    expect(screen.getByText('Public Content')).toBeInTheDocument();
    expect(screen.queryByText('Home Page')).not.toBeInTheDocument();
  });

  it('should redirect to home when user is authenticated', async () => {
    renderWithRouter(true);

    await screen.findByText('Home Page');
    expect(screen.getByText('Home Page')).toBeInTheDocument();
    expect(screen.queryByText('Public Content')).not.toBeInTheDocument();
  });
});
