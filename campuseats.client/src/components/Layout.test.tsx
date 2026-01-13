import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import Layout from './Layout';
import { AuthProvider } from '../context/AuthContext';
import { LanguageProvider } from '../context/LanguageContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');
jest.mock('./Header', () => ({
  __esModule: true,
  default: () => <div>Header Component</div>,
}));
jest.mock('./Sidebar', () => ({
  __esModule: true,
  default: () => <div>Sidebar Component</div>,
}));
jest.mock('./LanguageSelector', () => ({
  __esModule: true,
  default: () => <div>Language Selector</div>,
}));

const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('Layout', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render children', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    render(
      <LanguageProvider>
        <AuthProvider>
          <Layout>
            <div>Test Content</div>
          </Layout>
        </AuthProvider>
      </LanguageProvider>
    );

    await screen.findByText('Test Content');
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('should always render LanguageSelector', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    render(
      <LanguageProvider>
        <AuthProvider>
          <Layout>
            <div>Content</div>
          </Layout>
        </AuthProvider>
      </LanguageProvider>
    );

    await screen.findByText('Content');
    expect(screen.getByText('Language Selector')).toBeInTheDocument();
  });

  it('should not render Header and Sidebar when not authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    render(
      <LanguageProvider>
        <AuthProvider>
          <Layout>
            <div>Content</div>
          </Layout>
        </AuthProvider>
      </LanguageProvider>
    );

    await screen.findByText('Content');
    expect(screen.queryByText('Header Component')).not.toBeInTheDocument();
    expect(screen.queryByText('Sidebar Component')).not.toBeInTheDocument();
  });

  it('should render Header and Sidebar when authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
    } as Response);

    render(
      <LanguageProvider>
        <AuthProvider>
          <Layout>
            <div>Content</div>
          </Layout>
        </AuthProvider>
      </LanguageProvider>
    );

    await screen.findByText('Content');
    expect(screen.getByText('Header Component')).toBeInTheDocument();
    expect(screen.getByText('Sidebar Component')).toBeInTheDocument();
  });

  it('should apply padding to main when authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: true, role: 'Buyer' }),
    } as Response);

    const { container } = render(
      <LanguageProvider>
        <AuthProvider>
          <Layout>
            <div>Content</div>
          </Layout>
        </AuthProvider>
      </LanguageProvider>
    );

    await screen.findByText('Content');
    const main = container.querySelector('main');
    expect(main).toHaveClass('p-8');
  });

  it('should not apply padding to main when not authenticated', async () => {
    mockApiClient.mockResolvedValue({
      ok: true,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    const { container } = render(
      <LanguageProvider>
        <AuthProvider>
          <Layout>
            <div>Content</div>
          </Layout>
        </AuthProvider>
      </LanguageProvider>
    );

    await screen.findByText('Content');
    const main = container.querySelector('main');
    expect(main).not.toHaveClass('p-8');
  });
});
