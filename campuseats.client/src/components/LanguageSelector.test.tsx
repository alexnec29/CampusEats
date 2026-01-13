import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import LanguageSelector from './LanguageSelector'; // Asigură-te că importul e corect
import { LanguageProvider } from '../context/LanguageContext';

describe('LanguageSelector', () => {
  it('should render and switch languages', async () => {
    const user = userEvent.setup();
    render(
        <LanguageProvider>
          <LanguageSelector />
        </LanguageProvider>
    );

    // Folosim name pentru a distinge butoanele
    const roButton = screen.getByRole('button', { name: /RO/i });
    const enButton = screen.getByRole('button', { name: /EN/i });

    expect(roButton).toBeInTheDocument();
    expect(enButton).toBeInTheDocument();

    // Click EN
    await user.click(enButton);
    // Verifică clasa de activare (blue-600)
    expect(enButton).toHaveClass('bg-blue-600');
    expect(roButton).not.toHaveClass('bg-blue-600');
  });
});