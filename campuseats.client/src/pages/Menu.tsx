import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { MenuItem, Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const Menu: React.FC = () => {
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [loading, setLoading] = useState(true);
  const { isAuthenticated, userRole } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchMenu = async () => {
      try {
        const response = await apiClient('/api/menu-items');
        if (response.ok) {
          const data = await response.json();
          setMenuItems(data);
        }
      } catch (error) {
        console.error('Error fetching menu:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMenu();
  }, []);

  const addToOrder = async (item: MenuItem) => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    try {
      // 1. Get user orders to find pending one
      const ordersRes = await apiClient('/api/orders/my-orders');
      let pendingOrder: Order | undefined;

      if (ordersRes.ok) {
        const orders: Order[] = await ordersRes.json();
        pendingOrder = orders.find(o => o.status === OrderStatus.Pending);
      }

      // 2. If no pending order, create one
      if (!pendingOrder) {
        const createRes = await apiClient('/api/orders', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ notes: '' })
        });
        
        if (!createRes.ok) {
            if (createRes.status === 409) {
                // Conflict - order already exists. Try to get ID from response
                try {
                    const errorData = await createRes.json();
                    if (errorData.orderId) {
                        pendingOrder = { id: errorData.orderId } as Order; // Minimal order object
                    }
                } catch (e) {
                    console.error('Error parsing conflict response', e);
                }
                
                // If we still don't have it, try fetching one last time
                if (!pendingOrder) {
                     const retryRes = await apiClient('/api/orders/my-orders');
                     if (retryRes.ok) {
                        const orders: Order[] = await retryRes.json();
                        pendingOrder = orders.find(o => o.status === OrderStatus.Pending);
                     }
                }
            } else {
                alert('Failed to create order');
                return;
            }
        } else {
            // Created successfully
            // Fetch again to get the new order ID
            const ordersRes2 = await apiClient('/api/orders/my-orders');
            if (ordersRes2.ok) {
                const orders: Order[] = await ordersRes2.json();
                pendingOrder = orders.find(o => o.status === OrderStatus.Pending);
            }
        }
      }

      if (!pendingOrder) {
        alert('Could not create or find order.');
        return;
      }

      // 3. Add item to order
      const addRes = await apiClient(`/api/orders/${pendingOrder.id}/items`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          menuItemId: item.id,
          quantity: 1
        })
      });

      if (addRes.ok) {
        alert(`Added ${item.name} to order!`);
      } else {
        alert('Failed to add item.');
      }

    } catch (error) {
      console.error('Error adding to order:', error);
      alert('Error adding to order');
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this item?')) return;

    try {
      const response = await apiClient(`/api/menu-items/${id}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        setMenuItems(prev => prev.filter(item => item.id !== id));
      } else {
        alert('Failed to delete item');
      }
    } catch (error) {
      console.error('Error deleting item:', error);
      alert('Error deleting item');
    }
  };

  if (loading) return <div className="p-4">Loading menu...</div>;

  return (
    <div className="p-4">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Menu</h2>
        {(userRole === 'Buyer' || userRole === 'Admin') && (
            <button
                onClick={() => navigate('/cart')}
                className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
            >
                Go to Cart
            </button>
        )}
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {menuItems.map(item => (
          <div key={item.id} className="border rounded-lg p-4 shadow-sm hover:shadow-md transition-shadow">
            {item.imageUrl && (
              <img src={item.imageUrl} alt={item.name} className="w-full h-48 object-cover rounded-md mb-4" />
            )}
            <h3 className="text-xl font-semibold">{item.name}</h3>
            <p className="text-gray-600 mb-2">{item.description}</p>
            <div className="flex justify-between items-center mt-4">
              <span className="text-lg font-bold">${item.price.toFixed(2)}</span>
              
              <div className="flex space-x-2">
                {(userRole === 'Buyer' || userRole === 'Admin') && (
                  <button
                    onClick={() => addToOrder(item)}
                    className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
                    disabled={!item.isAvailable}
                  >
                    {item.isAvailable ? 'Add to Order' : 'Unavailable'}
                  </button>
                )}

                {(userRole === 'Kitchen' || userRole === 'Admin') && (
                  <button
                    onClick={() => handleDelete(item.id)}
                    className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
                  >
                    Delete
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Menu;
