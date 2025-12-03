import { getCsrfToken } from './csrf';

interface RequestOptions extends RequestInit {
  headers?: Record<string, string>;
}

export const apiClient = async (url: string, options: RequestOptions = {}) => {
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

  const response = await fetch(url, config);
  
  return response;
};
