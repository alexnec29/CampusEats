import { apiClient } from '../utils/apiClient';
import { UserInfo, PasswordChangeData } from '../types/profileTypes';

export const userService = {
    async checkAuth(): Promise<UserInfo | null> {
        const res = await apiClient('/api/user/check-auth');
        if (res.ok) {
            return await res.json();
        }
        return null;
    },

    async changePassword(data: PasswordChangeData): Promise<{ success: boolean; error?: string }> {
        const res = await apiClient('/api/user/change-password', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });

        if (res.ok) {
            return { success: true };
        } else {
            const text = await res.text();
            return { success: false, error: text || 'Eroare la schimbarea parolei.' };
        }
    },
};
