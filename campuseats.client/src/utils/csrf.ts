export const getCsrfToken = (): string | null => {
  const name = 'CSRF-TOKEN=';
  const decodedCookie = decodeURIComponent(document.cookie);
  const ca = decodedCookie.split(';');
  for (const part of ca) {
    let c = part;
    while (c.startsWith(' ')) {
      c = c.substring(1);
    }
    if (c.startsWith(name)) {
      return c.substring(name.length, c.length);
    }
  }
  return null;
};
