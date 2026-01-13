import { apiClient } from '../utils/apiClient';
import { BuyerProfile, KitchenProfile } from '../types/profileTypes';

export const profileService = {
    async getBuyerProfile(): Promise<BuyerProfile | null> {
        const res = await apiClient('/api/user/buyer-profile');
        if (res.ok) {
            return await res.json();
        } else if (res.status === 404) {
            return null;
        }
        throw new Error('Failed to load buyer profile');
    },

    async updateBuyerProfile(profile: BuyerProfile): Promise<{ success: boolean; error?: string }> {
        const res = await apiClient('/api/user/update-buyer-profile', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(profile),
        });

        if (res.ok || res.status === 204) {
            return { success: true };
        } else {
            const text = await res.text();
            return { success: false, error: text };
        }
    },

    async getKitchenProfile(): Promise<KitchenProfile | null> {
        const res = await apiClient('/api/user/kitchen-profile');
        if (res.ok) {
            return await res.json();
        } else if (res.status === 404) {
            return null;
        }
        throw new Error('Failed to load kitchen profile');
    },

    async updateKitchenProfile(profile: KitchenProfile): Promise<{ success: boolean; error?: string }> {
        const res = await apiClient('/api/user/update-kitchen-profile', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(profile),
        });

        if (res.ok || res.status === 204) {
            return { success: true };
        } else {
            const text = await res.text();
            return { success: false, error: text };
        }
    },

    async getLoyaltyPoints(): Promise<number | null> {
        try {
            const res = await apiClient('/api/loyalty/account');
            if (res.ok) {
                const data = await res.json();
                return data.pointsBalance;
            } else if (res.status === 404) {
                return null;
            }
            throw new Error('Failed to load loyalty points');
        } catch (err) {
            console.error('Error loading loyalty points:', err);
            return null;
        }
    },
};
