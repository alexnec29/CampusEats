import { apiClient } from '../utils/apiClient';
import { Order, OrderStatus } from '../types';

export const orderService = {
    async getMyOrders(): Promise<Order[]> {
        const response = await apiClient('/api/orders/my-orders');
        if (response.ok) {
            return await response.json();
        }
        throw new Error('Failed to fetch orders');
    },

    async getPendingOrder(): Promise<Order | null> {
        const orders = await this.getMyOrders();
        return orders.find(o => o.status === OrderStatus.Pending) || null;
    },

    async createOrder(notes: string = ''): Promise<Order> {
        const response = await apiClient('/api/orders', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ notes })
        });

        if (response.ok) {
            return await response.json();
        } else if (response.status === 409) {
            const errorData = await response.json();
            if (errorData.orderId) {
                const orders = await this.getMyOrders();
                const existingOrder = orders.find(o => o.id === errorData.orderId);
                if (existingOrder) return existingOrder;
            }
        }
        throw new Error('Failed to create order');
    },

    async updateItemQuantity(orderId: number, itemId: number, quantity: number): Promise<void> {
        const response = await apiClient(`/api/orders/${orderId}/items/${itemId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ quantity })
        });

        if (!response.ok) {
            throw new Error('Failed to update quantity');
        }
    },

    async removeItem(orderId: number, itemId: number): Promise<void> {
        const response = await apiClient(`/api/orders/${orderId}/items/${itemId}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error('Failed to remove item');
        }
    },

    async updateOrderStatus(orderId: number, status: OrderStatus): Promise<void> {
        const response = await apiClient(`/api/orders/${orderId}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status })
        });

        if (!response.ok) {
            throw new Error('Failed to update order status');
        }
    },

    async addItemToOrder(orderId: number, menuItemId: number, quantity: number): Promise<void> {
        const response = await apiClient(`/api/orders/${orderId}/items`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ menuItemId, quantity })
        });

        if (!response.ok) {
            throw new Error('Failed to add item to order');
        }
    },
};
