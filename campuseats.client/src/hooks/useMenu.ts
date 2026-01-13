import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MenuItem } from '../types';
import { menuService } from '../services/menuService';
import { orderService } from '../services/orderService';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';

export const useMenu = () => {
    const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
    const [loading, setLoading] = useState(true);
    const { isAuthenticated, userRole } = useAuth();
    const { showToast } = useToast();
    const { confirm } = useConfirm();
    const navigate = useNavigate();

    useEffect(() => {
        fetchMenu();
    }, []);

    const fetchMenu = async () => {
        try {
            const data = await menuService.getMenuItems();
            setMenuItems(data);
        } catch (error) {
            console.error('Error fetching menu:', error);
        } finally {
            setLoading(false);
        }
    };

    const getOrCreatePendingOrder = async () => {
        let pendingOrder = await orderService.getPendingOrder();

        if (!pendingOrder) {
            try {
                pendingOrder = await orderService.createOrder('');
            } catch (error) {
                pendingOrder = await orderService.getPendingOrder();
            }
        }

        return pendingOrder;
    };

    const addToOrder = async (item: MenuItem) => {
        if (!isAuthenticated) {
            navigate('/login');
            return;
        }

        try {
            const pendingOrder = await getOrCreatePendingOrder();

            if (!pendingOrder) {
                showToast('Nu s-a putut crea sau găsi comanda.', 'error');
                return;
            }

            await orderService.addItemToOrder(pendingOrder.id, item.id, 1);
            showToast(`${item.name} a fost adăugat în coș!`, 'success');
        } catch (error) {
            console.error('Error adding to order:', error);
            showToast('Eroare la adăugarea în coș', 'error');
        }
    };

    const handleDelete = async (id: number) => {
        const confirmed = await confirm({
            title: 'Șterge Produs',
            message: 'Ești sigur că vrei să ștergi acest produs din meniu?',
            confirmText: 'Șterge',
            type: 'danger'
        });

        if (!confirmed) return;

        try {
            await menuService.deleteMenuItem(id);
            setMenuItems(prev => prev.filter(item => item.id !== id));
            showToast('Produs șters cu succes', 'success');
        } catch (error) {
            console.error('Error deleting item:', error);
            showToast('Nu s-a putut șterge produsul', 'error');
        }
    };

    return {
        menuItems,
        loading,
        userRole,
        addToOrder,
        handleDelete,
        navigate,
    };
};
