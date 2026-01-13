import { getCsrfToken } from './csrf';

describe('getCsrfToken', () => {
  beforeEach(() => {
    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: '',
    });
  });

  it('should return the CSRF token when it exists in cookies', () => {
    document.cookie = 'CSRF-TOKEN=test-token-123';
    
    const token = getCsrfToken();
    
    expect(token).toBe('test-token-123');
  });

  it('should return null when CSRF token does not exist', () => {
    document.cookie = 'OTHER-COOKIE=some-value';
    
    const token = getCsrfToken();
    
    expect(token).toBeNull();
  });

  it('should return null when cookies are empty', () => {
    document.cookie = '';
    
    const token = getCsrfToken();
    
    expect(token).toBeNull();
  });

  it('should handle CSRF token with spaces before cookie name', () => {
    document.cookie = ' CSRF-TOKEN=token-with-space';
    
    const token = getCsrfToken();
    
    expect(token).toBe('token-with-space');
  });

  it('should return correct token when multiple cookies exist', () => {
    document.cookie = 'session=xyz; CSRF-TOKEN=my-token; user=john';
    
    const token = getCsrfToken();
    
    expect(token).toBe('my-token');
  });

  it('should handle encoded cookie values', () => {
    document.cookie = 'CSRF-TOKEN=token%20with%20space';
    
    const token = getCsrfToken();
    
    expect(token).toBe('token with space');
  });
});
