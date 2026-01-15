import React from 'react';
import { render, screen } from '@testing-library/react';
import { useNavigate, useLocation, Link, Navigate, BrowserRouter, Routes, Route } from './react-router-dom';

describe('react-router-dom mock', () => {
    it('useNavigate returns a jest function', () => {
        const navigate = useNavigate();
        expect(typeof navigate).toBe('function');
        const mockFn = navigate() as unknown as jest.Mock;
        // Verify it returns a mock that can be called
        // Note: The mock implementation returns jest.fn(), so navigate() returns a new mock function
    });

    it('useLocation returns default location', () => {
        const location = useLocation();
        expect(location).toEqual({ pathname: '/', state: null });
    });

    it('Link renders an anchor tag', () => {
        render(<Link to="/test-link">Click me</Link>);
        const link = screen.getByText('Click me');
        expect(link.tagName).toBe('A');
        expect(link).toHaveAttribute('href', '/test-link');
    });

    it('Navigate renders navigation message', () => {
        render(<Navigate to="/home" />);
        expect(screen.getByText('Navigating to /home')).toBeInTheDocument();
    });

    it('Router components render children', () => {
        render(
            <BrowserRouter>
                <Routes>
                    <Route element={<span>Test Element</span>} />
                </Routes>
            </BrowserRouter>
        );
        expect(screen.getByText('Test Element')).toBeInTheDocument();
    });
});
