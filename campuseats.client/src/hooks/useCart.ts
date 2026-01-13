import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Order, OrderStatus } from '../types';
import { orderService } from '../services/orderService';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';

export const useCart = () => {
    const [cart, setCart] = useState<Order | null>(null);
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
        fetchCart();
    }, [isAuthenticated, navigate]);

    const fetchCart = async () => {
        try {
            const pendingOrder = await orderService.getPendingOrder();
            setCart(pendingOrder);
        } catch (error) {
            console.error('Error fetching cart:', error);
        } finally {
            setLoading(false);
        }
    };

    const updateQuantity = async (itemId: number, newQuantity: number) => {
        if (!cart || newQuantity < 1) return;

        try {
            await orderService.updateItemQuantity(cart.id, itemId, newQuantity);
            await fetchCart();
        } catch (error) {
            console.error('Error updating quantity:', error);
            showToast('Failed to update quantity', 'error');
        }
    };

    const removeItem = async (itemId: number) => {
        if (!cart) return;

        const confirmed = await confirm({
            title: 'Șterge produs',
            message: 'Ești sigur că vrei să ștergi acest produs din coș?',
            confirmText: 'Șterge',
            type: 'danger'
        });

        if (!confirmed) return;

        try {
            await orderService.removeItem(cart.id, itemId);
            await fetchCart();
            showToast('Produs șters din coș', 'success');
        } catch (error) {
            console.error('Error removing item:', error);
            showToast('Eroare la ștergerea produsului', 'error');
        }
    };

    const placeOrder = async () => {
        if (!cart) return;

        const confirmed = await confirm({
            title: 'Plasează Comanda',
            message: 'Ești sigur că vrei să plasezi comanda?',
            confirmText: 'Plasează',
            type: 'info'
        });

        if (!confirmed) return;

        try {
            await orderService.updateOrderStatus(cart.id, OrderStatus.Placed);
            showToast('Comanda a fost plasată cu succes!', 'success');
            navigate('/payment', { state: { orderId: cart.id } });
        } catch (error) {
            console.error('Error placing order:', error);
            showToast('Eroare la plasarea comenzii', 'error');
        }
    };

    const hasItems = cart && cart.orderItems && cart.orderItems.length > 0;

    return {
        cart,
        loading,
        hasItems,
        updateQuantity,
        removeItem,
        placeOrder,
    };
};
