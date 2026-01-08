import { apiClient } from './apiClient';
import * as csrf from './csrf';

// Mock the csrf module
jest.mock('./csrf');

describe('apiClient', () => {
  const mockFetch = jest.fn();
  const originalFetch = global.fetch;

  beforeEach(() => {
    global.fetch = mockFetch;
    mockFetch.mockClear();
    (csrf.getCsrfToken as jest.Mock).mockReturnValue(null);
  });

  afterEach(() => {
    global.fetch = originalFetch;
  });

  test('makes a GET request with default headers', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: async () => ({ data: 'test' }),
    });

    await apiClient('/api/test');

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      headers: {
        'Accept': 'application/json',
      },
      credentials: 'include',
    });
  });

  test('includes CSRF token in headers when available', async () => {
    (csrf.getCsrfToken as jest.Mock).mockReturnValue('test-csrf-token');
    mockFetch.mockResolvedValue({ ok: true });

    await apiClient('/api/test');

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      headers: {
        'Accept': 'application/json',
        'X-CSRF-TOKEN': 'test-csrf-token',
      },
      credentials: 'include',
    });
  });

  test('adds Content-Type header for POST requests with body', async () => {
    mockFetch.mockResolvedValue({ ok: true });

    await apiClient('/api/test', {
      method: 'POST',
      body: JSON.stringify({ name: 'test' }),
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      method: 'POST',
      body: JSON.stringify({ name: 'test' }),
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });
  });

  test('does not override custom Content-Type header', async () => {
    mockFetch.mockResolvedValue({ ok: true });

    await apiClient('/api/test', {
      method: 'POST',
      body: 'custom body',
      headers: {
        'Content-Type': 'text/plain',
      },
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      method: 'POST',
      body: 'custom body',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'text/plain',
      },
      credentials: 'include',
    });
  });

  test('merges custom headers with default headers', async () => {
    mockFetch.mockResolvedValue({ ok: true });

    await apiClient('/api/test', {
      headers: {
        'Custom-Header': 'custom-value',
      },
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      headers: {
        'Accept': 'application/json',
        'Custom-Header': 'custom-value',
      },
      credentials: 'include',
    });
  });

  test('includes credentials in all requests', async () => {
    mockFetch.mockResolvedValue({ ok: true });

    await apiClient('/api/test');

    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        credentials: 'include',
      })
    );
  });

  test('returns the fetch response', async () => {
    const mockResponse = {
      ok: true,
      status: 200,
      json: async () => ({ data: 'test' }),
    };
    mockFetch.mockResolvedValue(mockResponse);

    const response = await apiClient('/api/test');

    expect(response).toBe(mockResponse);
  });

  test('handles fetch errors', async () => {
    const error = new Error('Network error');
    mockFetch.mockRejectedValue(error);

    await expect(apiClient('/api/test')).rejects.toThrow('Network error');
  });

  test('supports DELETE method', async () => {
    mockFetch.mockResolvedValue({ ok: true });

    await apiClient('/api/test/1', { method: 'DELETE' });

    expect(mockFetch).toHaveBeenCalledWith(
      '/api/test/1',
      expect.objectContaining({
        method: 'DELETE',
      })
    );
  });

  test('supports PUT method', async () => {
    mockFetch.mockResolvedValue({ ok: true });

    await apiClient('/api/test/1', {
      method: 'PUT',
      body: JSON.stringify({ name: 'updated' }),
    });

    expect(mockFetch).toHaveBeenCalledWith(
      '/api/test/1',
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify({ name: 'updated' }),
      })
    );
  });
});
