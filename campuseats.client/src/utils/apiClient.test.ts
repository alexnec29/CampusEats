import { apiClient } from './apiClient';
import * as csrfModule from './csrf';

global.fetch = jest.fn();

jest.mock('./csrf');

describe('apiClient', () => {
  const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>;
  const mockGetCsrfToken = csrfModule.getCsrfToken as jest.MockedFunction<typeof csrfModule.getCsrfToken>;

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should make a GET request with default headers', async () => {
    const mockResponse = { ok: true, json: async () => ({ data: 'test' }) } as Response;
    mockFetch.mockResolvedValue(mockResponse);
    mockGetCsrfToken.mockReturnValue(null);

    const response = await apiClient('/api/test');

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      headers: {
        'Accept': 'application/json',
      },
      credentials: 'include',
    });
    expect(response).toBe(mockResponse);
  });

  it('should include CSRF token when available', async () => {
    const mockResponse = { ok: true } as Response;
    mockFetch.mockResolvedValue(mockResponse);
    mockGetCsrfToken.mockReturnValue('test-csrf-token');

    await apiClient('/api/test');

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      headers: {
        'Accept': 'application/json',
        'X-CSRF-TOKEN': 'test-csrf-token',
      },
      credentials: 'include',
    });
  });

  it('should add Content-Type header when body is provided', async () => {
    const mockResponse = { ok: true } as Response;
    mockFetch.mockResolvedValue(mockResponse);
    mockGetCsrfToken.mockReturnValue(null);

    const body = JSON.stringify({ username: 'test' });
    await apiClient('/api/login', {
      method: 'POST',
      body,
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/login', {
      method: 'POST',
      body,
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });
  });

  it('should not override custom Content-Type header', async () => {
    const mockResponse = { ok: true } as Response;
    mockFetch.mockResolvedValue(mockResponse);
    mockGetCsrfToken.mockReturnValue(null);

    await apiClient('/api/upload', {
      method: 'POST',
      body: 'file-data',
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/upload', {
      method: 'POST',
      body: 'file-data',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'multipart/form-data',
      },
      credentials: 'include',
    });
  });

  it('should merge custom headers with default headers', async () => {
    const mockResponse = { ok: true } as Response;
    mockFetch.mockResolvedValue(mockResponse);
    mockGetCsrfToken.mockReturnValue('token-123');

    await apiClient('/api/test', {
      headers: {
        'X-Custom-Header': 'custom-value',
      },
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/test', {
      headers: {
        'Accept': 'application/json',
        'X-Custom-Header': 'custom-value',
        'X-CSRF-TOKEN': 'token-123',
      },
      credentials: 'include',
    });
  });

  it('should handle fetch errors', async () => {
    mockFetch.mockRejectedValue(new Error('Network error'));
    mockGetCsrfToken.mockReturnValue(null);

    await expect(apiClient('/api/test')).rejects.toThrow('Network error');
  });

  it('should always include credentials', async () => {
    const mockResponse = { ok: true } as Response;
    mockFetch.mockResolvedValue(mockResponse);
    mockGetCsrfToken.mockReturnValue(null);

    await apiClient('/api/test', { method: 'DELETE' });

    const callArgs = mockFetch.mock.calls[0][1];
    expect(callArgs?.credentials).toBe('include');
  });
});
