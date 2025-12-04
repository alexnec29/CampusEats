import { getCsrfToken } from './csrf';

const BASE_URL = "http://localhost:5267"; 

interface RequestOptions extends RequestInit {
  headers?: Record<string, string>;
}

export const apiClient = async (endpoint: string, options: RequestOptions = {}) => {
  const csrfToken = getCsrfToken();
  
  const headers: Record<string, string> = {
    'Accept': 'application/json',
    ...options.headers,
  };

  if (options.body && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json';
  }

  if (csrfToken) {
    headers['X-CSRF-TOKEN'] = csrfToken;
  }

  const config: RequestInit = {
    ...options,
    headers,
    credentials: 'include', 
  };

  const url = `${BASE_URL}${endpoint}`;

  const response = await fetch(url, config);
  
  return response;
};