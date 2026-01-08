import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { MemoryRouter } from 'react-router-dom';
import Login from './Login';
import { AuthProvider } from '../context/AuthContext';
import * as apiClient from '../utils/apiClient';

jest.mock('../utils/apiClient');

const mockApiClient = apiClient.apiClient as jest.MockedFunction<typeof apiClient.apiClient>;

const renderLogin = async (isAuthenticated: boolean = false) => {
  mockApiClient.mockResolvedValue({
    ok: isAuthenticated,
    json: async () => ({ isAuthenticated, role: isAuthenticated ? 'Buyer' : null }),
  } as Response);

  const result = render(
    <MemoryRouter>
      <AuthProvider>
        <Login />
      </AuthProvider>
    </MemoryRouter>
  );

  await new Promise(resolve => setTimeout(resolve, 50));

  return result;
};

describe('Login Page', () => {
  beforeEach(() => {
    mockApiClient.mockClear();
    mockNavigate.mockClear();
  });

  test('renders login form with all elements', async () => {
    await renderLogin();

    expect(screen.getByText('Bine ai venit înapoi!')).toBeInTheDocument();
    expect(screen.getByText('Intră în contul tău CampusEats')).toBeInTheDocument();
    expect(screen.getByLabelText('Username')).toBeInTheDocument();
    expect(screen.getByLabelText('Parolă')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
  });

  test('displays link to register page', async () => {
    await renderLogin();

    const registerLink = screen.getByText(/Înregistrează-te/i);
    expect(registerLink).toBeInTheDocument();
    expect(registerLink.closest('a')).toHaveAttribute('href', '/register');
  });

  test('username input accepts text input', async () => {
    await renderLogin();

    const usernameInput = screen.getByLabelText('Username') as HTMLInputElement;
    await userEvent.type(usernameInput, 'testuser');

    expect(usernameInput.value).toBe('testuser');
  });

  test('password input accepts text input', async () => {
    await renderLogin();

    const passwordInput = screen.getByLabelText('Parolă') as HTMLInputElement;
    await userEvent.type(passwordInput, 'password123');

    expect(passwordInput.value).toBe('password123');
  });

  test('password input has type password', async () => {
    await renderLogin();

    const passwordInput = screen.getByLabelText('Parolă');
    expect(passwordInput).toHaveAttribute('type', 'password');
  });

  test('submits form with valid credentials', async () => {
    mockApiClient.mockResolvedValueOnce({
      ok: false,
      json: async () => ({ isAuthenticated: false, role: null }),
    } as Response);

    mockApiClient.mockResolvedValueOnce({
      ok: true,
      text: async () => 'Login successful',
    } as Response);

    await renderLogin();

    const usernameInput = screen.getByLabelText('Username');
    const passwordInput = screen.getByLabelText('Parolă');
    const submitButton = screen.getByRole('button', { name: /login/i });

    await userEvent.type(usernameInput, 'testuser');
    await userEvent.type(passwordInput, 'password123');
    await userEvent.click(submitButton);

    await waitFor(() => {
      expect(mockApiClient).toHaveBeenCalledWith('/api/user/login', {
        method: 'POST',
        body: JSON.stringify({
          username: 'testuser',
          password: 'password123',
        }),
      });
    });
  });

  test('displays error message when login fails', async () => {
    mockApiClient
      .mockResolvedValueOnce({
        ok: false,
        json: async () => ({ isAuthenticated: false, role: null }),
      } as Response)
      .mockResolvedValueOnce({
        ok: false,
        text: async () => 'Invalid credentials',
      } as Response);

    await renderLogin();

    const usernameInput = screen.getByLabelText('Username');
    const passwordInput = screen.getByLabelText('Parolă');
    const submitButton = screen.getByRole('button', { name: /login/i });

    await userEvent.type(usernameInput, 'wronguser');
    await userEvent.type(passwordInput, 'wrongpass');
    await userEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    });
  });

  test('displays generic error message on network error', async () => {
    mockApiClient
      .mockResolvedValueOnce({
        ok: false,
        json: async () => ({ isAuthenticated: false, role: null }),
      } as Response)
      .mockRejectedValueOnce(new Error('Network error'));

    await renderLogin();

    const usernameInput = screen.getByLabelText('Username');
    const passwordInput = screen.getByLabelText('Parolă');
    const submitButton = screen.getByRole('button', { name: /login/i });

    await userEvent.type(usernameInput, 'testuser');
    await userEvent.type(passwordInput, 'password123');
    await userEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('An error occurred. Please try again.')).toBeInTheDocument();
    });
  });

  test('both inputs are required', async () => {
    await renderLogin();

    const usernameInput = screen.getByLabelText('Username');
    const passwordInput = screen.getByLabelText('Parolă');

    expect(usernameInput).toBeRequired();
    expect(passwordInput).toBeRequired();
  });

  test('clears error message when form is resubmitted', async () => {
    mockApiClient
      .mockResolvedValueOnce({
        ok: false,
        json: async () => ({ isAuthenticated: false, role: null }),
      } as Response)
      .mockResolvedValueOnce({
        ok: false,
        text: async () => 'Invalid credentials',
      } as Response)
      .mockResolvedValueOnce({
        ok: true,
        text: async () => 'Login successful',
      } as Response);

    await renderLogin();

    const usernameInput = screen.getByLabelText('Username');
    const passwordInput = screen.getByLabelText('Parolă');
    const submitButton = screen.getByRole('button', { name: /login/i });

    // First attempt - fail
    await userEvent.type(usernameInput, 'wronguser');
    await userEvent.type(passwordInput, 'wrongpass');
    await userEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    });

    // Clear inputs
    await userEvent.clear(usernameInput);
    await userEvent.clear(passwordInput);

    // Second attempt
    await userEvent.type(usernameInput, 'correctuser');
    await userEvent.type(passwordInput, 'correctpass');
    await userEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.queryByText('Invalid credentials')).not.toBeInTheDocument();
    });
  });

  test('has correct placeholder text', async () => {
    await renderLogin();

    const usernameInput = screen.getByPlaceholderText('Introdu username-ul');
    const passwordInput = screen.getByPlaceholderText('Introdu parola');

    expect(usernameInput).toBeInTheDocument();
    expect(passwordInput).toBeInTheDocument();
  });
});
