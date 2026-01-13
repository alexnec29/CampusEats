import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Order, OrderStatus } from '../types';
import { orderService } from '../services/orderService';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';

const STATUS_LABELS: Record<OrderStatus, string> = {
    [OrderStatus.Inactive]: 'Inactive',
    [OrderStatus.Pending]: 'Cart',
    [OrderStatus.Placed]: 'Placed',
    [OrderStatus.Paid]: 'Paid',
    [OrderStatus.Preparing]: 'Preparing',
    [OrderStatus.Ready]: 'Ready',
    [OrderStatus.Completed]: 'Completed',
    [OrderStatus.Cancelled]: 'Cancelled',
    [OrderStatus.PendingPayment]: 'PendingPayment',
    [OrderStatus.FailedPayment]: 'FailedPayment',
};

const STATUS_COLORS: Record<OrderStatus, string> = {
    [OrderStatus.Inactive]: 'bg-gray-200 text-gray-600',
    [OrderStatus.Pending]: 'bg-yellow-100 text-yellow-800',
    [OrderStatus.Placed]: 'bg-blue-100 text-blue-800',
    [OrderStatus.Paid]: 'bg-green-100 text-green-800',
    [OrderStatus.Preparing]: 'bg-purple-100 text-purple-800',
    [OrderStatus.Ready]: 'bg-green-100 text-green-800',
    [OrderStatus.Completed]: 'bg-gray-100 text-gray-800',
    [OrderStatus.Cancelled]: 'bg-red-100 text-red-800',
    [OrderStatus.PendingPayment]: 'bg-yellow-100 text-yellow-800',
    [OrderStatus.FailedPayment]: 'bg-red-100 text-red-800',
};

export const useOrders = () => {
    const [orders, setOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const { isAuthenticated } = useAuth();
    const { showToast } = useToast();
    const { confirm } = useConfirm();
    const navigate = useNavigate();

    useEffect(() => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }
        fetchOrders();
    }, [isAuthenticated, navigate]);

    const fetchOrders = async () => {
        try {
            const data = await orderService.getMyOrders();
            const placedOrders = data.filter(o => o.status !== OrderStatus.Pending);
            const sorted = placedOrders.sort((a, b) =>
                new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()
            );
            setOrders(sorted);
        } catch (error) {
            console.error('Error fetching orders:', error);
        } finally {
            setLoading(false);
        }
    };

    const removeItem = async (orderId: number, itemId: number) => {
        const confirmed = await confirm({
            title: 'Șterge produs',
            message: 'Ești sigur că vrei să ștergi acest produs?',
            confirmText: 'Șterge',
            type: 'danger'
        });

        if (!confirmed) return;

        try {
            await orderService.removeItem(orderId, itemId);
            await fetchOrders();
            showToast('Produs șters cu succes', 'success');
        } catch (error) {
            console.error('Error removing item:', error);
            showToast('Eroare la ștergerea produsului', 'error');
        }
    };

    const getStatusLabel = (status: OrderStatus): string => {
        return STATUS_LABELS[status] || 'Unknown';
    };

    const getStatusColor = (status: OrderStatus): string => {
        return STATUS_COLORS[status] || 'bg-gray-100 text-gray-800';
    };

    return {
        orders,
        loading,
        removeItem,
        getStatusLabel,
        getStatusColor,
    };
};
