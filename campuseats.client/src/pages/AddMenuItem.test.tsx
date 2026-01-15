import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import AddMenuItem from './AddMenuItem';
import { ToastProvider } from '../context/ToastContext';
import * as apiClientModule from '../utils/apiClient';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
    useNavigate: () => mockNavigate,
}));

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

describe('AddMenuItem Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should render all form fields correctly', () => {
        render(
            <ToastProvider>
                <AddMenuItem />
            </ToastProvider>
        );

        expect(screen.getByText(/Add New Menu Item/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Name/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Description/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Price/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Category/i)).toBeInTheDocument();
    });

    it('should submit the form with correct data types', async () => {
        mockApiClient.mockResolvedValueOnce({ ok: true } as Response);

        render(
            <ToastProvider>
                <AddMenuItem />
            </ToastProvider>
        );

        fireEvent.change(screen.getByLabelText(/Name/i), { target: { value: 'Pizza Margherita' } });
        fireEvent.change(screen.getByLabelText(/Description/i), { target: { value: 'Classic pizza' } });
        fireEvent.change(screen.getByLabelText(/Price/i), { target: { value: '25.50' } });
        fireEvent.change(screen.getByLabelText(/Category/i), { target: { value: '1' } }); // Lunch = 1

        fireEvent.click(screen.getByRole('button', { name: /Add Item/i }));

        await waitFor(() => {
            expect(mockApiClient).toHaveBeenCalledWith('/api/menu-items', expect.objectContaining({
                method: 'POST',
                body: JSON.stringify({
                    name: 'Pizza Margherita',
                    description: 'Classic pizza',
                    price: 25.5,
                    category: 1,
                    imageUrl: '',
                    isAvailable: true
                })
            }));
            expect(mockNavigate).toHaveBeenCalledWith('/menu');
        });
    });

    it('should display error message when server returns error', async () => {
        mockApiClient.mockResolvedValueOnce({ ok: false } as Response);

        render(
            <ToastProvider>
                <AddMenuItem />
            </ToastProvider>
        );

        fireEvent.change(screen.getByLabelText(/Name/i), { target: { value: 'Test' } });
        fireEvent.click(screen.getByRole('button', { name: /Add Item/i }));

        await waitFor(() => {
            expect(screen.getByText(/Nu s-a putut adăuga produsul/i)).toBeInTheDocument();
        });
    });
});