import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import Profile from './Profile';
import { ToastProvider } from '../context/ToastContext';
import { ConfirmProvider } from '../context/ConfirmContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');
const mockApiClient = apiClientModule.apiClient as jest.MockedFunction<typeof apiClientModule.apiClient>;

const fullBuyerProfile = {
    firstName: 'Ion',
    lastName: 'Popescu',
    age: 30,
    deliveryAddress: {
        street: 'Strada Principala',
        building: '10',
        city: 'Bucuresti',
        county: 'Sector 1'
    }
};

const fullKitchenProfile = {
    companyName: 'Pizza Buna',
    kitchenAddress: {
        street: 'Strada Cuptorului',
        building: '1',
        city: 'Iasi',
        county: 'Iasi'
    },
    weeklyWorkingHours: {
        monday: { open: '09:00', close: '22:00' },
        tuesday: { open: '09:00', close: '22:00' },
        wednesday: { open: '09:00', close: '22:00' },
        thursday: { open: '09:00', close: '22:00' },
        friday: { open: '09:00', close: '23:00' },
        saturday: { open: '10:00', close: '23:00' },
        sunday: { open: '10:00', close: '22:00' }
    }
};

describe('Profile Page', () => {
    beforeEach(() => {
        jest.clearAllMocks();

        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ username: 'ClientTest', role: 'Buyer', email: 'client@test.com' })
                } as Response);
            }
            if (url.includes('/api/user/buyer-profile')) {
                return Promise.resolve({ ok: true, json: async () => fullBuyerProfile } as Response);
            }
            if (url.includes('/api/user/kitchen-profile')) {
                return Promise.resolve({ ok: true, json: async () => fullKitchenProfile } as Response);
            }
            if (url.includes('/api/loyalty/account')) {
                return Promise.resolve({ ok: true, json: async () => ({ pointsBalance: 100 }) } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => ({}) } as Response);
        });
    });

    const renderProfile = () => render(
        <ToastProvider>
            <ConfirmProvider>
                <Profile />
            </ConfirmProvider>
        </ToastProvider>
    );

    it('renders and displays Buyer information without crashing', async () => {
        renderProfile();

        expect(await screen.findByText('ClientTest')).toBeInTheDocument();
        expect(screen.getByText('Profil cumpărător')).toBeInTheDocument();
        expect(screen.getByText(/Strada Principala/i)).toBeInTheDocument();
        expect(screen.getByText(/Puncte loialitate:/i)).toBeInTheDocument();
        expect(screen.getByText('100')).toBeInTheDocument();
    });

    it('renders Kitchen profile information correctly', async () => {
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ username: 'ChefTest', role: 'Kitchen' })
                } as Response);
            }
            if (url.includes('/api/user/kitchen-profile')) {
                return Promise.resolve({ ok: true, json: async () => fullKitchenProfile } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => ({}) } as Response);
        });

        renderProfile();

        expect(await screen.findByText('ChefTest')).toBeInTheDocument();
        expect(screen.getByText('Profil bucătărie')).toBeInTheDocument();
        expect(screen.getByText('Pizza Buna')).toBeInTheDocument();
    });

    it('handles non-existent buyer profile (404)', async () => {
        mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({ ok: true, json: async () => ({ username: 'ClientNew', role: 'Buyer' }) } as Response);
            }
            if (url.includes('/api/user/buyer-profile')) {
                return Promise.resolve({ ok: false, status: 404 } as Response);
            }
             if (url.includes('/api/loyalty/account')) {
                return Promise.resolve({ ok: true, json: async () => ({ pointsBalance: 0 }) } as Response);
            }
            return Promise.resolve({ ok: true } as Response);
        });

        renderProfile();
        expect(await screen.findByText('ClientNew')).toBeInTheDocument();
        expect(screen.getByText(/Nu ați creat încă un profil de cumpărător/i)).toBeInTheDocument();
    });

    it('allows changing password successfully', async () => {
        renderProfile();

        // Open modal
        const changePassBtn = await screen.findByText(/Schimbă parola/i);
        fireEvent.click(changePassBtn);

        expect(screen.getByText('Schimbare parolă')).toBeInTheDocument(); 

        // Fill form
        fireEvent.change(screen.getByPlaceholderText(/Parola curentă/i), { target: { value: 'oldPass' } });
        fireEvent.change(screen.getByPlaceholderText('Parola nouă'), { target: { value: 'newPass123' } }); 
        fireEvent.change(screen.getByPlaceholderText('Confirmă parola nouă'), { target: { value: 'newPass123' } });

        // Mock success response
        mockApiClient.mockImplementation((url: string, options) => {
            if (url.includes('change-password') && options?.method === 'PUT') {
                 return Promise.resolve({ ok: true } as Response);
            }
            // default calls
             if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ username: 'test', role: 'Buyer' }) } as Response);
             if (url.includes('buyer-profile')) return Promise.resolve({ ok: true, json: async () => fullBuyerProfile } as Response);
             if (url.includes('loyalty')) return Promise.resolve({ ok: true, json: async () => ({ pointsBalance: 0 }) } as Response);
             return Promise.resolve({ ok: true } as Response);
        });

        // Submit - button text is "Confirmă"
        const saveBtn = screen.getByRole('button', { name: 'Confirmă' });
        fireEvent.click(saveBtn);
        
        await waitFor(() => {
            expect(screen.queryByText('Schimbare parolă')).not.toBeInTheDocument();
        });
    });

    it('shows error when password confirmation mismatches', async () => {
        renderProfile();

        const changePassBtn = await screen.findByText(/Schimbă parola/i);
        fireEvent.click(changePassBtn);

        fireEvent.change(screen.getByPlaceholderText(/Parola curentă/i), { target: { value: 'oldPass' } });
        fireEvent.change(screen.getByPlaceholderText('Parola nouă'), { target: { value: 'newPass' } });
        fireEvent.change(screen.getByPlaceholderText('Confirmă parola nouă'), { target: { value: 'wrongConfirm' } });

        fireEvent.click(screen.getByRole('button', { name: 'Confirmă' }));

        expect(await screen.findByText(/Noua parolă și confirmarea nu coincid/i)).toBeInTheDocument();
    });

    it('allows editing buyer profile', async () => {
        renderProfile();

        const editBtn = await screen.findByRole('button', { name: /Editează/i });
        fireEvent.click(editBtn);

        // Wait for modal
        expect(await screen.findByText('Editează profil cumpărător')).toBeInTheDocument();

        const firstNameInput = screen.getByPlaceholderText('Prenume');
        fireEvent.change(firstNameInput, { target: { value: 'Ionel' } });

        mockApiClient.mockImplementation((url: string, options) => {
            if (url.includes('update-buyer-profile') && options?.method === 'PUT') {
                return Promise.resolve({ ok: true } as Response);
            }
             // default calls
             if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ username: 'test', role: 'Buyer' }) } as Response);
             if (url.includes('buyer-profile')) {
                 // Return the updated profile on reload
                 return Promise.resolve({ ok: true, json: async () => ({ ...fullBuyerProfile, firstName: 'Ionel' }) } as Response);
            }
             if (url.includes('loyalty')) return Promise.resolve({ ok: true, json: async () => ({ pointsBalance: 0 }) } as Response);
             return Promise.resolve({ ok: true } as Response);
        });

        // Use getByRole for the button "Salvează"
        const saveBtn = screen.getByRole('button', { name: 'Salvează' });
        fireEvent.click(saveBtn);
        
        // Modal closes
        await waitFor(() => {
             expect(screen.queryByText('Editează profil cumpărător')).not.toBeInTheDocument();
        });
    });

    it('allows editing kitchen profile', async () => {
         mockApiClient.mockImplementation((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ username: 'ChefTest', role: 'Kitchen' })
                } as Response);
            }
            if (url.includes('/api/user/kitchen-profile')) {
                return Promise.resolve({ ok: true, json: async () => fullKitchenProfile } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => ({}) } as Response);
        });

        renderProfile();

        const editBtn = await screen.findByRole('button', { name: /Editează/i });
        fireEvent.click(editBtn);

        expect(await screen.findByText('Editează profil bucătărie')).toBeInTheDocument();

        const companyInput = screen.getByPlaceholderText(/Nume companie/i);
        fireEvent.change(companyInput, { target: { value: 'Pizza Delicioasa' } });

        mockApiClient.mockImplementation((url: string, options) => {
            if (url.includes('update-kitchen-profile') && options?.method === 'PUT') {
                 return Promise.resolve({ ok: true } as Response);
            }
            // Defaults
            if (url.includes('check-auth')) return Promise.resolve({ ok: true, json: async () => ({ username: 'ChefTest', role: 'Kitchen' }) } as Response);
            if (url.includes('kitchen-profile')) return Promise.resolve({ ok: true, json: async () => ({...fullKitchenProfile, companyName: 'Pizza Delicioasa'}) } as Response);
            return Promise.resolve({ ok: true } as Response);
        });

        fireEvent.click(screen.getByRole('button', { name: 'Salvează' }));

        await waitFor(() => {
            expect(screen.queryByText('Editează profil bucătărie')).not.toBeInTheDocument();
        });
    });

    it('opens password change modal when clicking the button', async () => {
        renderProfile();

        const changePassBtn = await screen.findByText(/Schimbă parola/i);
        fireEvent.click(changePassBtn);

        expect(screen.getByPlaceholderText('Parola curentă')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Parola nouă')).toBeInTheDocument();
    });

    it('shows confirmation when trying to delete account', async () => {
        renderProfile();

        const deleteBtn = await screen.findByText(/Șterge contul/i);
        fireEvent.click(deleteBtn);

        // Check for the unique message in the dialog
        expect(screen.getByText(/Funcționalitatea de ștergere cont nu este încă implementată/i)).toBeInTheDocument();
    });
});