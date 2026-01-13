import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { LanguageProvider, useLanguage, Language } from './LanguageContext';

const TestComponent = () => {
  const { language, setLanguage } = useLanguage();
  
  return (
    <div>
      <div data-testid="current-language">{language}</div>
      <button onClick={() => setLanguage('en')}>Switch to English</button>
      <button onClick={() => setLanguage('ro')}>Switch to Romanian</button>
    </div>
  );
};

describe('LanguageContext', () => {
  it('should throw error when useLanguage is used outside LanguageProvider', () => {
    const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
    
    expect(() => render(<TestComponent />)).toThrow('useLanguage must be used within a LanguageProvider');
    
    consoleError.mockRestore();
  });

  it('should provide default language as "ro"', () => {
    render(
      <LanguageProvider>
        <TestComponent />
      </LanguageProvider>
    );

    expect(screen.getByTestId('current-language')).toHaveTextContent('ro');
  });

  it('should allow switching to English', async () => {
    const user = userEvent.setup();

    render(
      <LanguageProvider>
        <TestComponent />
      </LanguageProvider>
    );

    const button = screen.getByRole('button', { name: /switch to english/i });
    await user.click(button);

    expect(screen.getByTestId('current-language')).toHaveTextContent('en');
  });

  it('should allow switching to Romanian', async () => {
    const user = userEvent.setup();

    render(
      <LanguageProvider>
        <TestComponent />
      </LanguageProvider>
    );

    const englishButton = screen.getByRole('button', { name: /switch to english/i });
    await user.click(englishButton);

    expect(screen.getByTestId('current-language')).toHaveTextContent('en');

    const romanianButton = screen.getByRole('button', { name: /switch to romanian/i });
    await user.click(romanianButton);

    expect(screen.getByTestId('current-language')).toHaveTextContent('ro');
  });

  it('should maintain language state across component updates', async () => {
    const user = userEvent.setup();

    const { rerender } = render(
      <LanguageProvider>
        <TestComponent />
      </LanguageProvider>
    );

    const button = screen.getByRole('button', { name: /switch to english/i });
    await user.click(button);

    expect(screen.getByTestId('current-language')).toHaveTextContent('en');

    rerender(
      <LanguageProvider>
        <TestComponent />
      </LanguageProvider>
    );

    expect(screen.getByTestId('current-language')).toHaveTextContent('ro');
  });
});
