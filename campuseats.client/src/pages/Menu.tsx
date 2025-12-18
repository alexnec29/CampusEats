import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { MenuItem, Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';
import { useNavigate } from 'react-router-dom';

const Menu: React.FC = () => {
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [loading, setLoading] = useState(true);
  const { isAuthenticated, userRole } = useAuth();
  const { showToast } = useToast();
  const { confirm } = useConfirm();
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
                showToast('Nu s-a putut crea comanda', 'error');
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
        showToast('Nu s-a putut crea sau găsi comanda.', 'error');
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
        showToast(`${item.name} a fost adăugat în coș!`, 'success');
      } else {
        showToast('Nu s-a putut adăuga produsul.', 'error');
      }

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
      const response = await apiClient(`/api/menu-items/${id}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        setMenuItems(prev => prev.filter(item => item.id !== id));
        showToast('Produs șters cu succes', 'success');
      } else {
        showToast('Nu s-a putut șterge produsul', 'error');
      }
    } catch (error) {
      console.error('Error deleting item:', error);
      showToast('Eroare la ștergerea produsului', 'error');
    }
  };

  if (loading) return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 flex justify-center items-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
    </div>
  );

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 p-6 page-transition">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
            <div>
                <h2 className="text-4xl font-bold text-gray-900 mb-2">Meniu</h2>
                <p className="text-gray-600">Descoperă preparatele noastre delicioase</p>
            </div>
            {(userRole === 'Buyer' || userRole === 'Admin') && (
                <button
                    onClick={() => navigate('/cart')}
                    className="bg-gradient-to-r from-green-500 to-emerald-600 text-white px-6 py-3 rounded-xl hover:from-green-600 hover:to-emerald-700 font-bold shadow-lg transform hover:scale-105 transition duration-300 flex items-center"
                >
                    <span className="mr-2">🛒</span> Vezi Coșul
                </button>
            )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {menuItems.map(item => (
            <div key={item.id} className="bg-white rounded-2xl shadow-lg overflow-hidden transform transition-all duration-300 hover:shadow-2xl hover:-translate-y-2">
                <div className="relative h-48 bg-gray-200">
                    {item.imageUrl ? (
                        <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                    ) : (
                        <div className="w-full h-full flex items-center justify-center text-gray-400">
                            <span className="text-4xl">🍽️</span>
                        </div>
                    )}
                    {!item.isAvailable && (
                        <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center">
                            <span className="bg-red-500 text-white px-4 py-1 rounded-full font-bold transform -rotate-12">Indisponibil</span>
                        </div>
                    )}
                </div>
                
                <div className="p-6">
                    <div className="flex justify-between items-start mb-2">
                        <h3 className="text-xl font-bold text-gray-900">{item.name}</h3>
                        <span className="text-2xl font-bold text-blue-600">${item.price.toFixed(2)}</span>
                    </div>
                    
                    <p className="text-gray-600 mb-6 line-clamp-2">{item.description}</p>
                    
                    <div className="flex space-x-3">
                        {(userRole === 'Buyer' || userRole === 'Admin') && (
                        <button
                            onClick={() => addToOrder(item)}
                            className={`flex-1 py-3 px-4 rounded-xl font-bold shadow-md transition-all duration-300 transform active:scale-95 ${
                                item.isAvailable 
                                ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white hover:from-blue-700 hover:to-purple-700 hover:shadow-lg' 
                                : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                            }`}
                            disabled={!item.isAvailable}
                        >
                            {item.isAvailable ? 'Adaugă în Coș' : 'Indisponibil'}
                        </button>
                        )}

                        {(userRole === 'Kitchen' || userRole === 'Admin') && (
                        <button
                            onClick={() => handleDelete(item.id)}
                            className="bg-red-100 text-red-600 p-3 rounded-xl hover:bg-red-200 transition-colors"
                            title="Delete Item"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                        </button>
                        )}
                    </div>
                </div>
            </div>
            ))}
        </div>
      </div>
    </div>
  );
};

export default Menu;
