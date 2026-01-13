import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Order, OrderStatus } from '../types';
import { orderService } from '../services/orderService';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';
import { getOrderStatusLabel, getOrderStatusColor } from '../utils/orderUtils';

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

    return {
        orders,
        loading,
        removeItem,
        getStatusLabel: getOrderStatusLabel,
        getStatusColor: getOrderStatusColor,
    };
};
