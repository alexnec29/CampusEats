import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import Landing from './Landing';
import { LanguageProvider } from '../context/LanguageContext';

jest.mock('react-router-dom', () => ({
    Link: ({ children, to }: any) => <a href={to}>{children}</a>,
}));

jest.mock('lucide-react', () => ({
    UtensilsCrossed: () => <div data-testid="icon-utensils" />,
    ShoppingBag: () => <div data-testid="icon-shopping" />,
    Clock: () => <div data-testid="icon-clock" />,
    Award: () => <div data-testid="icon-award" />,
    Globe: () => <div data-testid="icon-globe" />,
}));

describe('Landing Page', () => {
    const renderLanding = () => render(
        <LanguageProvider>
            <Landing />
        </LanguageProvider>
    );

    it('should render the welcome title correctly', () => {
        renderLanding();
        expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent(/Bine ai venit la CampusEats/i);
    });

    it('should display the login CTA button', () => {
        renderLanding();
        const loginLink = screen.getByRole('link', { name: /intra in cont/i });
        expect(loginLink).toBeInTheDocument();
        expect(loginLink).toHaveAttribute('href', '/login');
    });

    it('should render all feature sections', () => {
        renderLanding();
        expect(screen.getByTestId('icon-utensils')).toBeInTheDocument();
        expect(screen.getByTestId('icon-clock')).toBeInTheDocument();
        expect(screen.getByTestId('icon-shopping')).toBeInTheDocument();
        expect(screen.getByTestId('icon-award')).toBeInTheDocument();
    });

    it('should render the "How it works" section', () => {
        renderLanding();
        expect(screen.getByRole('heading', { level: 2, name: /cum funcționeaza/i })).toBeInTheDocument();
        expect(screen.getByText('1')).toBeInTheDocument();
        expect(screen.getByText('2')).toBeInTheDocument();
        expect(screen.getByText('3')).toBeInTheDocument();
    });

    it('should display the register CTA at the bottom', () => {
        renderLanding();
        const registerLink = screen.getByRole('link', { name: /inregistre-te gratuit/i });
        expect(registerLink).toBeInTheDocument();
        expect(registerLink).toHaveAttribute('href', '/register');
    });
});