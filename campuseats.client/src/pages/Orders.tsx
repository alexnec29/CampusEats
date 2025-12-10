import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
        navigate('/login');
        return;
    }

    const fetchOrders = async () => {
      try {
        const response = await apiClient('/api/orders/my-orders');
        if (response.ok) {
          const data: Order[] = await response.json();
          // Filter out Pending orders (Cart)
          const placedOrders = data.filter(o => o.status !== OrderStatus.Pending);
          
          // Sort by date descending
          const sorted = placedOrders.sort((a: Order, b: Order) => 
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          );
          setOrders(sorted);
        }
      } catch (error) {
        console.error('Error fetching orders:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, [isAuthenticated, navigate]);

  const getStatusLabel = (status: OrderStatus) => {
    switch (status) {
        case OrderStatus.Inactive: return 'Inactive';
        case OrderStatus.Pending: return 'Cart'; // Should not be seen here usually
        case OrderStatus.Placed: return 'Placed';
        case OrderStatus.Preparing: return 'Preparing';
        case OrderStatus.Ready: return 'Ready';
        case OrderStatus.Completed: return 'Completed';
        case OrderStatus.Cancelled: return 'Cancelled';
        default: return 'Unknown';
    }
  };

  const getStatusColor = (status: OrderStatus) => {
      switch (status) {
          case OrderStatus.Inactive: return 'bg-gray-200 text-gray-600';
          case OrderStatus.Pending: return 'bg-yellow-100 text-yellow-800';
          case OrderStatus.Placed: return 'bg-blue-100 text-blue-800';
          case OrderStatus.Preparing: return 'bg-purple-100 text-purple-800';
          case OrderStatus.Ready: return 'bg-green-100 text-green-800';
          case OrderStatus.Completed: return 'bg-gray-100 text-gray-800';
          case OrderStatus.Cancelled: return 'bg-red-100 text-red-800';
          default: return 'bg-gray-100 text-gray-800';
      }
  };

  const removeItem = async (orderId: number, itemId: number) => {
    if (!window.confirm('Are you sure you want to remove this item?')) return;

    try {
      const response = await apiClient(`/api/orders/${orderId}/items/${itemId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        // Refresh orders
        const ordersRes = await apiClient('/api/orders/my-orders');
        if (ordersRes.ok) {
          const data = await ordersRes.json();
          const sorted = data.sort((a: Order, b: Order) => 
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          );
          setOrders(sorted);
        }
      } else {
        alert('Failed to remove item');
      }
    } catch (error) {
      console.error('Error removing item:', error);
      alert('Error removing item');
    }
  };

  if (loading) return <div className="p-4">Loading orders...</div>;

  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold mb-6">My Orders</h2>
      {orders.length === 0 ? (
        <p>No orders found.</p>
      ) : (
        <div className="space-y-4">
          {orders.map(order => (
            <div key={order.id} className="border rounded-lg p-4 shadow-sm">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <span className="text-sm text-gray-500">Order #{order.id}</span>
                  <p className="text-sm text-gray-500">{new Date(order.createdAt).toLocaleString()}</p>
                </div>
                <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(order.status)}`}>
                  {getStatusLabel(order.status)}
                </span>
              </div>
              
              <div className="border-t pt-4">
                <h4 className="font-semibold mb-2">Items:</h4>
                <ul className="space-y-2">
                  {order.orderItems?.map(item => (
                    <li key={item.id} className="flex justify-between text-sm items-center">
                      <span>{item.quantity}x {item.menuItem?.name || 'Unknown Item'}</span>
                      <div className="flex items-center gap-4">
                        <span>${(item.price * item.quantity).toFixed(2)}</span>
                        {order.status === OrderStatus.Pending && (
                            <button 
                                onClick={() => removeItem(order.id, item.id)}
                                className="text-red-600 hover:text-red-800 text-xs font-semibold"
                            >
                                Remove
                            </button>
                        )}
                      </div>
                    </li>
                  ))}
                </ul>
                <div className="border-t mt-4 pt-2 flex justify-between font-bold">
                  <span>Total</span>
                  <span>${order.totalAmount.toFixed(2)}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Orders;
