import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { LanguageProvider, useLanguage } from './LanguageContext';

const TestComponent = () => {
  const { language, setLanguage } = useLanguage();
  return (
      <div>
        <div data-testid="lang-display">{language}</div>
        <button onClick={() => setLanguage('en')}>Set EN</button>
        <button onClick={() => setLanguage('ro')}>Set RO</button>
      </div>
  );
};

describe('LanguageContext', () => {
  it('should provide default language (ro)', () => {
    render(
        <LanguageProvider>
          <TestComponent />
        </LanguageProvider>
    );
    expect(screen.getByTestId('lang-display')).toHaveTextContent('ro');
  });

  it('should switch languages correctly', async () => {
    const user = userEvent.setup();
    render(
        <LanguageProvider>
          <TestComponent />
        </LanguageProvider>
    );

    // Switch to EN
    await user.click(screen.getByText('Set EN'));
    expect(screen.getByTestId('lang-display')).toHaveTextContent('en');

    // Switch back to RO
    await user.click(screen.getByText('Set RO'));
    expect(screen.getByTestId('lang-display')).toHaveTextContent('ro');
  });
});