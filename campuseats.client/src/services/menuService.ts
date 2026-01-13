import { apiClient } from '../utils/apiClient';
import { MenuItem } from '../types';

export const menuService = {
    async getMenuItems(): Promise<MenuItem[]> {
        const response = await apiClient('/api/menu-items');
        if (response.ok) {
            return await response.json();
        }
        throw new Error('Failed to fetch menu items');
    },

    async deleteMenuItem(id: number): Promise<void> {
        const response = await apiClient(`/api/menu-items/${id}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error('Failed to delete menu item');
        }
    },
};
