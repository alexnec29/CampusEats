import React from 'react';
import { render, screen } from '@testing-library/react';
import { useNavigate, useLocation, Link, Navigate, BrowserRouter, Routes, Route } from './react-router-dom';

describe('react-router-dom mock', () => {
    it('useNavigate returns a jest function', () => {
        const navigate = useNavigate();
        expect(typeof navigate).toBe('function');
        // It's a jest mock function, so we can check properties if needed, but type check is enough for coverage
        expect(jest.isMockFunction(navigate)).toBe(true);
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

    it('Navigate renders text indicating navigation', () => {
        render(<Navigate to="/destination" />);
        expect(screen.getByText('Navigating to /destination')).toBeInTheDocument();
    });

    it('BrowserRouter renders children', () => {
        render(<BrowserRouter><span>Child</span></BrowserRouter>);
        expect(screen.getByText('Child')).toBeInTheDocument();
    });

    it('Routes renders children', () => {
        render(<Routes><span>Route Child</span></Routes>);
        expect(screen.getByText('Route Child')).toBeInTheDocument();
    });

    it('Route renders element', () => {
        render(<Route element={<span>Route Element</span>} />);
        expect(screen.getByText('Route Element')).toBeInTheDocument();
    });
});
