import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import LanguageSelector from './LanguageSelector'; // Asigură-te că importul e corect
import { LanguageProvider } from '../context/LanguageContext';

describe('LanguageSelector', () => {
  it('should render and switch languages', async () => {
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
    fireEvent.click(enButton);
    // Verifică clasa de activare (blue-600)
    expect(enButton).toHaveClass('bg-blue-600');
    expect(roButton).not.toHaveClass('bg-blue-600');
  });
});