import { getCsrfToken } from './csrf';

describe('getCsrfToken', () => {
  beforeEach(() => {
    // Clear all cookies before each test
    document.cookie.split(";").forEach((c) => {
      document.cookie = c
        .replace(/^ +/, "")
        .replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
    });
  });

  test('returns null when no CSRF token cookie exists', () => {
    const token = getCsrfToken();
    expect(token).toBeNull();
  });

  test('returns the CSRF token when cookie exists', () => {
    document.cookie = 'CSRF-TOKEN=test-token-123';
    const token = getCsrfToken();
    expect(token).toBe('test-token-123');
  });

  test('returns the correct token when multiple cookies exist', () => {
    document.cookie = 'other-cookie=value1';
    document.cookie = 'CSRF-TOKEN=my-csrf-token';
    document.cookie = 'another-cookie=value2';
    const token = getCsrfToken();
    expect(token).toBe('my-csrf-token');
  });

  test('handles cookies with spaces correctly', () => {
    document.cookie = ' CSRF-TOKEN=token-with-space';
    const token = getCsrfToken();
    expect(token).toBe('token-with-space');
  });

  test('returns null when CSRF-TOKEN cookie has no value', () => {
    document.cookie = 'CSRF-TOKEN=';
    const token = getCsrfToken();
    expect(token).toBe('');
  });

  test('handles encoded cookie values', () => {
    const encodedValue = encodeURIComponent('token=with=special=chars');
    document.cookie = `CSRF-TOKEN=${encodedValue}`;
    const token = getCsrfToken();
    expect(token).toBe('token=with=special=chars');
  });
});
