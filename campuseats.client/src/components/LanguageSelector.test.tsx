import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import LanguageSelector from './LanguageSelector';
import { LanguageProvider } from '../context/LanguageContext';

describe('LanguageSelector', () => {
  it('should render language selector with globe icon', () => {
    render(
      <LanguageProvider>
        <LanguageSelector />
      </LanguageProvider>
    );

    const globeButton = screen.getByRole('button');
    expect(globeButton).toBeInTheDocument();
  });

  it('should display RO and EN buttons', () => {
    render(
      <LanguageProvider>
        <LanguageSelector />
      </LanguageProvider>
    );

    expect(screen.getByRole('button', { name: /RO/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /EN/i })).toBeInTheDocument();
  });

  it('should highlight RO button by default', () => {
    render(
      <LanguageProvider>
        <LanguageSelector />
      </LanguageProvider>
    );

    const roButton = screen.getByRole('button', { name: /RO/i });
    expect(roButton).toHaveClass('bg-blue-600', 'text-white');
  });

  it('should switch to English when EN button is clicked', async () => {
    const user = userEvent.setup();

    render(
      <LanguageProvider>
        <LanguageSelector />
      </LanguageProvider>
    );

    const enButton = screen.getByRole('button', { name: /EN/i });
    await user.click(enButton);

    expect(enButton).toHaveClass('bg-blue-600', 'text-white');
  });

  it('should switch back to Romanian when RO button is clicked', async () => {
    const user = userEvent.setup();

    render(
      <LanguageProvider>
        <LanguageSelector />
      </LanguageProvider>
    );

    const enButton = screen.getByRole('button', { name: /EN/i });
    await user.click(enButton);

    const roButton = screen.getByRole('button', { name: /RO/i });
    await user.click(roButton);

    expect(roButton).toHaveClass('bg-blue-600', 'text-white');
  });

  it('should remove highlight from RO when EN is selected', async () => {
    const user = userEvent.setup();

    render(
      <LanguageProvider>
        <LanguageSelector />
      </LanguageProvider>
    );

    const roButton = screen.getByRole('button', { name: /RO/i });
    const enButton = screen.getByRole('button', { name: /EN/i });

    await user.click(enButton);

    expect(roButton).toHaveClass('text-gray-600', 'hover:bg-gray-100');
    expect(roButton).not.toHaveClass('bg-blue-600');
  });
});
