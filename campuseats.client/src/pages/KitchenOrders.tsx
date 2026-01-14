import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useNavigate } from 'react-router-dom';

type OrderCardProps = {
    order: Order;
    actionLabel: string;
    onUpdateStatus: () => void;
    onCancel: () => void;
};

const OrderCard: React.FC<OrderCardProps> = ({ order, actionLabel, onUpdateStatus, onCancel }) => (
    <div className="bg-white p-4 rounded shadow mb-4 border-l-4 border-blue-500">
        <div className="flex justify-between items-start mb-2">
            <span className="font-bold text-lg">#{order.id}</span>
            <span className="text-sm text-gray-500">{new Date(order.orderDate).toLocaleTimeString()}</span>
        </div>
        <div className="mb-4">
            <ul className="text-sm">
                {order.orderItems?.map(item => (
                    <li key={item.id} className="flex justify-between">
                        <span>{item.quantity}x {item.menuItem?.name || 'Unknown'}</span>
                    </li>
                ))}
            </ul>
            {order.notes && <p className="text-xs text-gray-500 mt-2 italic">Note: {order.notes}</p>}
        </div>
        <div className="flex flex-row gap-3">
            <button
                onClick={onUpdateStatus}
                className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700 transition text-sm font-semibold"
            >
                {actionLabel}
            </button>
            <button
                onClick={onCancel}
                className="w-full bg-red-100 text-red-600 py-2 rounded hover:bg-red-200 transition text-sm font-semibold"
            >
                Cancel
            </button>
        </div>
    </div>
);

const KitchenOrders: React.FC = () => {
    const [paidOrders, setPaidOrders] = useState<Order[]>([]);
    const [preparingOrders, setPreparingOrders] = useState<Order[]>([]);
    const [readyOrders, setReadyOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const { userRole } = useAuth();
    const { showToast } = useToast();
    const navigate = useNavigate();

    useEffect(() => {
        if (userRole !== 'Kitchen' && userRole !== 'Admin') {
            navigate('/');
            return;
        }
        fetchOrders();
    }, [userRole, navigate]);

    const fetchOrders = async () => {
        setLoading(true);
        try {
            const [paidRes, preparingRes, readyRes] = await Promise.all([
                apiClient('/api/orders/status?status=Paid'),
                apiClient('/api/orders/status?status=Preparing'),
                apiClient('/api/orders/status?status=Ready')
            ]);

            if (paidRes.ok) setPaidOrders(await paidRes.json());
            if (preparingRes.ok) setPreparingOrders(await preparingRes.json());
            if (readyRes.ok) setReadyOrders(await readyRes.json());

        } catch (error) {
            console.error('Error fetching kitchen orders:', error);
        } finally {
            setLoading(false);
        }
    };

    const updateStatus = async (orderId: number, newStatus: OrderStatus) => {
        try {
            const response = await apiClient(`/api/orders/${orderId}/status`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ status: newStatus })
            });

            if (response.ok) {
                moveOrder(orderId, newStatus);
                showToast(`Status actualizat la ${OrderStatus[newStatus]}`, 'success');
            } else {
                showToast('Nu s-a putut actualiza statusul', 'error');
            }
        } catch (error) {
            console.error('Error updating status:', error);
            showToast('Eroare la actualizarea statusului', 'error');
        }
    };

    const cancelOrder = async (orderId: number) => {
        try {
            const response = await apiClient(`/api/orders/cancel-by-kitchen`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ orderId: orderId })
            });
            if (response.ok) {
                showToast('Comandă anulată cu succes', 'success');
                removeOrderFromCurrentList(orderId)
            } else {
                showToast('Comanda este deja anulată', 'warning');
            }
        } catch (error) {
            console.error('Error cancelling order:', error);
            showToast('Eroare la anularea comenzii', 'error');
        }
    };

    const moveOrder = (orderId: number, newStatus: OrderStatus) => {
        let order: Order | undefined;

        order = removeOrderFromCurrentList(orderId)

        if (order) {
            const updatedOrder = { ...order, status: newStatus };
            if (newStatus === OrderStatus.Preparing) {
                setPreparingOrders(prev => [...prev, updatedOrder]);
            } else if (newStatus === OrderStatus.Ready) {
                setReadyOrders(prev => [...prev, updatedOrder]);
            }
        }
    };

    const removeOrderFromCurrentList = (orderId: number) => {
        let order: Order | undefined;
        if (paidOrders.some(o => o.id === orderId)) {
            order = paidOrders.find(o => o.id === orderId);
            setPaidOrders(prev => prev.filter(o => o.id !== orderId));
        } else if (preparingOrders.some(o => o.id === orderId)) {
            order = preparingOrders.find(o => o.id === orderId);
            setPreparingOrders(prev => prev.filter(o => o.id !== orderId));
        } else if (readyOrders.some(o => o.id === orderId)) {
            order = readyOrders.find(o => o.id === orderId);
            setReadyOrders(prev => prev.filter(o => o.id !== orderId));
        }
        return order
    }

    if (loading) {
        return (
            <output className="flex justify-center items-center h-64" aria-label="loading">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500" />
            </output>
        );
    }

    return (
        <div className="p-6">
            <h2 className="text-3xl font-bold mb-8 text-gray-800">Kitchen Dashboard</h2>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="bg-gray-100 p-4 rounded-lg min-h-[500px]">
                    <h3 className="text-xl font-bold mb-4 text-gray-700 flex items-center">
                        <span className="w-3 h-3 bg-green-500 rounded-full mr-2"></span>
                        {" "}Incoming (Paid)
                        <span className="ml-auto bg-gray-200 text-gray-600 text-sm px-2 py-1 rounded-full">{paidOrders.length}</span>
                    </h3>
                    <div className="space-y-4">
                        {paidOrders.length === 0 && <p className="text-gray-400 text-center italic">No new orders</p>}
                        {paidOrders.map(order => (
                            <OrderCard
                                key={order.id}
                                order={order}
                                actionLabel="Start Preparing"
                                onUpdateStatus={() => updateStatus(order.id, OrderStatus.Preparing)}
                                onCancel={() => cancelOrder(order.id)}
                            />
                        ))}
                    </div>
                </div>

                <div className="bg-gray-100 p-4 rounded-lg min-h-[500px]">
                    <h3 className="text-xl font-bold mb-4 text-gray-700 flex items-center">
                        <span className="w-3 h-3 bg-yellow-500 rounded-full mr-2"></span>
                        {" "}Preparing
                        <span className="ml-auto bg-gray-200 text-gray-600 text-sm px-2 py-1 rounded-full">{preparingOrders.length}</span>
                    </h3>
                    <div className="space-y-4">
                        {preparingOrders.length === 0 && <p className="text-gray-400 text-center italic">No orders in prep</p>}
                        {preparingOrders.map(order => (
                            <OrderCard
                                key={order.id}
                                order={order}
                                actionLabel="Mark Ready"
                                onUpdateStatus={() => updateStatus(order.id, OrderStatus.Ready)}
                                onCancel={() => cancelOrder(order.id)}
                            />
                        ))}
                    </div>
                </div>

                <div className="bg-gray-100 p-4 rounded-lg min-h-[500px]">
                    <h3 className="text-xl font-bold mb-4 text-gray-700 flex items-center">
                        <span className="w-3 h-3 bg-blue-500 rounded-full mr-2"></span>
                        {" "}Ready for Pickup
                        <span className="ml-auto bg-gray-200 text-gray-600 text-sm px-2 py-1 rounded-full">{readyOrders.length}</span>
                    </h3>
                    <div className="space-y-4">
                        {readyOrders.length === 0 && <p className="text-gray-400 text-center italic">No ready orders</p>}
                        {readyOrders.map(order => (
                            <OrderCard
                                key={order.id}
                                order={order}
                                actionLabel="Complete Order"
                                onUpdateStatus={() => updateStatus(order.id, OrderStatus.Completed)}
                                onCancel={() => cancelOrder(order.id)}
                            />
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default KitchenOrders;