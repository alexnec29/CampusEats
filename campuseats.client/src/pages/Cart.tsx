import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const Cart: React.FC = () => {
  const [cart, setCart] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const fetchCart = async () => {
    try {
      const response = await apiClient('/api/orders/my-orders');
      if (response.ok) {
        const orders: Order[] = await response.json();
        const pendingOrder = orders.find(o => o.status === OrderStatus.Pending);
        setCart(pendingOrder || null);
      }
    } catch (error) {
      console.error('Error fetching cart:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!isAuthenticated) {
        navigate('/login');
        return;
    }
    fetchCart();
  }, [isAuthenticated, navigate]);

  const removeItem = async (itemId: number) => {
    if (!cart) return;
    if (!window.confirm('Are you sure you want to remove this item?')) return;

    try {
      const response = await apiClient(`/api/orders/${cart.id}/items/${itemId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        fetchCart(); // Refresh cart
      } else {
        alert('Failed to remove item');
      }
    } catch (error) {
      console.error('Error removing item:', error);
      alert('Error removing item');
    }
  };

  const placeOrder = async () => {
    if (!cart) return;
    if (!window.confirm('Are you sure you want to place this order?')) return;

    try {
      const response = await apiClient(`/api/orders/${cart.id}/status`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: OrderStatus.Placed })
      });

      if (response.ok) {
        alert('Order placed successfully! Redirecting to payment...');
        navigate('/payment', { state: { orderId: cart.id } });
      } else {
        alert('Failed to place order');
      }
    } catch (error) {
      console.error('Error placing order:', error);
      alert('Error placing order');
    }
  };

  if (loading) return <div className="p-4">Loading cart...</div>;

  if (!cart || !cart.orderItems || cart.orderItems.length === 0) {
    return (
        <div className="p-4 text-center">
            <h2 className="text-2xl font-bold mb-4">Your Cart</h2>
            <p className="text-gray-600">Your cart is empty.</p>
            <button 
                onClick={() => navigate('/menu')}
                className="mt-4 bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
            >
                Go to Menu
            </button>
        </div>
    );
  }

  return (
    <div className="p-4 max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold mb-6">Your Cart</h2>
      
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="p-4 border-b">
            <div className="flex justify-between items-center">
                <span className="font-semibold">Order #{cart.id}</span>
                <span className="text-sm text-gray-500">Status: Pending</span>
            </div>
        </div>
        
        <ul className="divide-y divide-gray-200">
            {cart.orderItems.map(item => (
                <li key={item.id} className="p-4 flex justify-between items-center">
                    <div>
                        <h4 className="font-medium">{item.menuItem?.name || 'Unknown Item'}</h4>
                        <p className="text-sm text-gray-500">Quantity: {item.quantity}</p>
                    </div>
                    <div className="flex items-center space-x-4">
                        <span className="font-bold">${(item.price * item.quantity).toFixed(2)}</span>
                        <button 
                            onClick={() => removeItem(item.id)}
                            className="text-red-600 hover:text-red-800"
                        >
                            Remove
                        </button>
                    </div>
                </li>
            ))}
        </ul>

        <div className="p-4 bg-gray-50 border-t flex justify-between items-center">
            <div className="text-xl font-bold">
                Total: ${cart.totalAmount.toFixed(2)}
            </div>
            <button
                onClick={placeOrder}
                className="bg-green-600 text-white px-6 py-2 rounded hover:bg-green-700 font-semibold"
            >
                Place Order
            </button>
        </div>
      </div>
    </div>
  );
};

export default Cart;
