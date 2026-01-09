import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import Header from './Header';

describe('Header Component', () => {
    test('renders the logo text CampusEats', () => {
        render(<Header />);
        const logoElement = screen.getByText(/CampusEats/i);
        expect(logoElement).toBeInTheDocument();
    });

    test('has the correct CSS classes', () => {
        const { container } = render(<Header />);
        const headerElement = container.querySelector('header');
        expect(headerElement).toHaveClass('bg-gray-50');
        expect(headerElement).toHaveClass('border-b');
    });
});