import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom';
import Layout from './Layout';
import { AuthProvider } from '../context/AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

const renderWithRouter = (component: React.ReactElement, isAuthenticated: boolean = false) => {
  mockApiClient.mockResolvedValue({
    ok: isAuthenticated,
    json: async () => ({ isAuthenticated, role: isAuthenticated ? 'Buyer' : null }),
  } as Response);

  return render(
    <BrowserRouter>
      <AuthProvider>
        {component}
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('Layout Component', () => {
  beforeEach(() => {
    mockApiClient.mockClear();
  });

  test('renders children correctly', async () => {
    renderWithRouter(
      <Layout>
        <div>Test Content</div>
      </Layout>,
      false
    );

    expect(await screen.findByText('Test Content')).toBeInTheDocument();
  });

  test('does not show Header and Sidebar when not authenticated', async () => {
    renderWithRouter(
      <Layout>
        <div>Test Content</div>
      </Layout>,
      false
    );

    // Wait for auth check
    await screen.findByText('Test Content');

    expect(screen.queryByText('CampusEats')).not.toBeInTheDocument();
    expect(screen.queryByText('Home')).not.toBeInTheDocument();
  });

  test('shows Header and Sidebar when authenticated', async () => {
    renderWithRouter(
      <Layout>
        <div>Test Content</div>
      </Layout>,
      true
    );

    // Wait for auth check and components to render
    await screen.findByText('Test Content');
    
    expect(await screen.findByText('CampusEats')).toBeInTheDocument();
    expect(screen.getByText('Home')).toBeInTheDocument();
  });

  test('applies flex layout structure', async () => {
    const { container } = renderWithRouter(
      <Layout>
        <div>Test Content</div>
      </Layout>,
      false
    );

    await screen.findByText('Test Content');

    const outerDiv = container.firstChild as HTMLElement;
    expect(outerDiv).toHaveClass('flex');
    expect(outerDiv).toHaveClass('min-h-screen');
  });

  test('applies correct padding to main when authenticated', async () => {
    renderWithRouter(
      <Layout>
        <div>Test Content</div>
      </Layout>,
      true
    );

    await screen.findByText('Test Content');

    const mainElement = screen.getByText('Test Content').parentElement;
    expect(mainElement).toHaveClass('p-8');
  });

  test('does not apply padding to main when not authenticated', async () => {
    renderWithRouter(
      <Layout>
        <div>Test Content</div>
      </Layout>,
      false
    );

    await screen.findByText('Test Content');

    const mainElement = screen.getByText('Test Content').parentElement;
    expect(mainElement).not.toHaveClass('p-8');
  });

  test('renders multiple children correctly', async () => {
    renderWithRouter(
      <Layout>
        <div>First Child</div>
        <div>Second Child</div>
        <div>Third Child</div>
      </Layout>,
      false
    );

    expect(await screen.findByText('First Child')).toBeInTheDocument();
    expect(screen.getByText('Second Child')).toBeInTheDocument();
    expect(screen.getByText('Third Child')).toBeInTheDocument();
  });
});
