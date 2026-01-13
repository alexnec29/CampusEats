import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
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
    });

    it('renders Kitchen profile information correctly', async () => {
        mockApiClient.mockImplementationOnce((url: string) => {
            if (url.includes('/api/user/check-auth')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => ({ username: 'ChefTest', role: 'Kitchen' })
                } as Response);
            }
            return Promise.resolve({ ok: true, json: async () => ({}) } as Response);
        });

        renderProfile();

        expect(await screen.findByText('ChefTest')).toBeInTheDocument();
        expect(screen.getByText('Profil bucătărie')).toBeInTheDocument();
        expect(screen.getByText('Pizza Buna')).toBeInTheDocument();
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

        expect(screen.getByText(/Ești sigur că vrei să continui/i)).toBeInTheDocument();
    });
});