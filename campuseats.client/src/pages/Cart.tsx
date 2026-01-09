import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';
import { useNavigate } from 'react-router-dom';

const Cart: React.FC = () => {
  const [cart, setCart] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [loyaltyPoints, setLoyaltyPoints] = useState<number>(0);
  const [pointsToRedeem, setPointsToRedeem] = useState<number>(0);
  const [applyingPoints, setApplyingPoints] = useState(false);
  const { isAuthenticated } = useAuth();
  const { showToast } = useToast();
  const { confirm } = useConfirm();
  const navigate = useNavigate();

  const fetchCart = async () => {
    try {
      const response = await apiClient('/api/orders/my-orders');
      if (response.ok) {
        const orders: Order[] = await response.json();
        const pendingOrder = orders.find(o => o.status === OrderStatus.Pending);
        setCart(pendingOrder || null);
        if (pendingOrder && pendingOrder.redeemedLoyaltyPoints) {
          setPointsToRedeem(pendingOrder.redeemedLoyaltyPoints);
        }
      }
    } catch (error) {
      console.error('Error fetching cart:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchLoyaltyPoints = async () => {
    try {
      const response = await apiClient('/api/loyalty/account');
      if (response.ok) {
        const data = await response.json();
        setLoyaltyPoints(data.pointsBalance);
      }
    } catch (error) {
      console.error('Error fetching loyalty points:', error);
    }
  };

  useEffect(() => {
    if (!isAuthenticated) {
        navigate('/login');
        return;
    }
    fetchCart();
    fetchLoyaltyPoints();
  }, [isAuthenticated, navigate]);

  const updateQuantity = async (itemId: number, newQuantity: number) => {
    if (!cart) return;
    if (newQuantity < 1) return;

    try {
      const response = await apiClient(`/api/orders/${cart.id}/items/${itemId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ quantity: newQuantity })
      });

      if (response.ok) {
        fetchCart(); // Refresh cart
      } else {
        showToast('Failed to update quantity', 'error');
      }
    } catch (error) {
      console.error('Error updating quantity:', error);
      showToast('Error updating quantity', 'error');
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
      const response = await apiClient(`/api/orders/${cart.id}/items/${itemId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        fetchCart(); // Refresh cart
        showToast('Produs șters din coș', 'success');
      } else {
        showToast('Nu s-a putut șterge produsul', 'error');
      }
    } catch (error) {
      console.error('Error removing item:', error);
      showToast('Eroare la ștergerea produsului', 'error');
    }
  };

  const applyLoyaltyPoints = async () => {
    if (!cart) return;
    
    if (pointsToRedeem < 0) {
      showToast('Punctele trebuie să fie un număr pozitiv', 'error');
      return;
    }

    setApplyingPoints(true);
    try {
      const response = await apiClient(`/api/orders/${cart.id}/apply-loyalty-points`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ points: pointsToRedeem })
      });

      if (response.ok) {
        const data = await response.json();
        showToast(`Discount aplicat: $${data.loyaltyPointsDiscount.toFixed(2)}`, 'success');
        await fetchCart();
        await fetchLoyaltyPoints();
      } else {
        const errorData = await response.json();
        showToast(errorData.message || 'Nu s-a putut aplica discountul', 'error');
      }
    } catch (error) {
      console.error('Error applying loyalty points:', error);
      showToast('Eroare la aplicarea punctelor', 'error');
    } finally {
      setApplyingPoints(false);
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
      const response = await apiClient(`/api/orders/${cart.id}/status`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: OrderStatus.Placed })
      });

      if (response.ok) {
        showToast('Comanda a fost plasată cu succes!', 'success');
        navigate('/payment', { state: { orderId: cart.id } });
      } else {
        showToast('Nu s-a putut plasa comanda', 'error');
      }
    } catch (error) {
      console.error('Error placing order:', error);
      showToast('Eroare la plasarea comenzii', 'error');
    }
  };

  if (loading) return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 flex justify-center items-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
    </div>
  );

  if (!cart || !cart.orderItems || cart.orderItems.length === 0) {
    return (
        <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-2xl shadow-2xl p-8 md:p-10 text-center max-w-md w-full transform transition-all duration-300 hover:shadow-3xl">
                <div className="text-6xl mb-6">🛒</div>
                <h2 className="text-3xl font-bold text-gray-900 mb-4">Coșul tău este gol</h2>
                <p className="text-gray-600 mb-8">Nu ai adăugat încă niciun produs în coș.</p>
                <button 
                    onClick={() => navigate('/menu')}
                    className="w-full bg-gradient-to-r from-blue-600 to-purple-600 text-white font-bold p-4 rounded-lg hover:from-blue-700 hover:to-purple-700 transform hover:scale-105 transition duration-300 shadow-lg"
                >
                    Vezi Meniul
                </button>
            </div>
        </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 p-6 page-transition">
      <div className="max-w-4xl mx-auto">
        <h2 className="text-3xl font-bold text-gray-900 mb-8 flex items-center">
            <span className="mr-3">🛒</span> Coșul Tău
        </h2>
      
        <div className="bg-white rounded-2xl shadow-2xl overflow-hidden transform transition-all duration-300 hover:shadow-3xl">
            <div className="p-6 border-b bg-gray-50">
                <div className="flex justify-between items-center">
                    <span className="font-bold text-lg text-gray-700">Comanda #{cart.id}</span>
                    <span className="px-4 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium">
                        Status: Pending
                    </span>
                </div>
            </div>
            
            <ul className="divide-y divide-gray-100">
                {cart.orderItems.map(item => (
                    <li key={item.id} className="p-6 flex flex-col sm:flex-row justify-between items-center hover:bg-gray-50 transition-colors duration-200">
                        <div className="flex-1">
                            <h4 className="text-xl font-semibold text-gray-800 mb-2">{item.menuItem?.name || 'Unknown Item'}</h4>
                            <div className="flex items-center mt-2">
                                <button 
                                    onClick={() => updateQuantity(item.id, item.quantity - 1)}
                                    className="w-8 h-8 flex items-center justify-center bg-gray-200 rounded-full hover:bg-gray-300 text-gray-700 transition-colors disabled:opacity-50"
                                    disabled={item.quantity <= 1}
                                >
                                    -
                                </button>
                                <span className="mx-4 font-semibold text-gray-700 w-8 text-center">
                                    {item.quantity}
                                </span>
                                <button 
                                    onClick={() => updateQuantity(item.id, item.quantity + 1)}
                                    className="w-8 h-8 flex items-center justify-center bg-gray-200 rounded-full hover:bg-gray-300 text-gray-700 transition-colors"
                                >
                                    +
                                </button>
                            </div>
                        </div>
                        <div className="flex items-center space-x-6 mt-4 sm:mt-0">
                            <span className="text-xl font-bold text-gray-900">${(item.price * item.quantity).toFixed(2)}</span>
                            <button 
                                onClick={() => removeItem(item.id)}
                                className="text-red-500 hover:text-red-700 p-2 hover:bg-red-50 rounded-full transition-all"
                                title="Remove item"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                </svg>
                            </button>
                        </div>
                    </li>
                ))}
            </ul>

            {/* Loyalty Points Section */}
            <div className="p-6 bg-gradient-to-r from-purple-50 to-blue-50 border-t border-b">
                <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-gray-800 flex items-center">
                        <span className="mr-2">⭐</span> Puncte de Loialitate
                    </h3>
                    <span className="text-sm text-gray-600">
                        Disponibile: <span className="font-bold text-purple-600">{loyaltyPoints}</span> puncte
                    </span>
                </div>
                
                {loyaltyPoints > 0 && (
                    <div className="space-y-4">
                        <div className="flex flex-col sm:flex-row items-center gap-4">
                            <div className="flex-1 w-full">
                                <label className="block text-sm text-gray-600 mb-2">
                                    Puncte de folosit (1 punct = $0.01):
                                </label>
                                <input
                                    type="number"
                                    min="0"
                                    max={loyaltyPoints}
                                    value={pointsToRedeem}
                                    onChange={(e) => setPointsToRedeem(Math.max(0, Math.min(loyaltyPoints, parseInt(e.target.value) || 0)))}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                                    placeholder="0"
                                />
                            </div>
                            <button
                                onClick={applyLoyaltyPoints}
                                disabled={applyingPoints || pointsToRedeem === 0}
                                className={`w-full sm:w-auto px-6 py-2 rounded-lg font-semibold transition-all ${
                                    applyingPoints || pointsToRedeem === 0
                                        ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                                        : 'bg-purple-600 text-white hover:bg-purple-700 shadow-md hover:shadow-lg'
                                }`}
                            >
                                {applyingPoints ? 'Se aplică...' : 'Aplică Discount'}
                            </button>
                        </div>
                        
                        {pointsToRedeem > 0 && (
                            <div className="text-sm text-gray-700 bg-white p-3 rounded-lg">
                                <p>💰 Discount estimat: <span className="font-bold text-green-600">${(pointsToRedeem * 0.01).toFixed(2)}</span></p>
                            </div>
                        )}
                        
                        {cart.redeemedLoyaltyPoints && cart.redeemedLoyaltyPoints > 0 && (
                            <div className="text-sm bg-green-100 text-green-800 p-3 rounded-lg border border-green-200">
                                ✅ Discount aplicat: <span className="font-bold">{cart.redeemedLoyaltyPoints} puncte</span> = <span className="font-bold">${cart.loyaltyPointsDiscount?.toFixed(2)}</span>
                            </div>
                        )}
                    </div>
                )}
                
                {loyaltyPoints === 0 && (
                    <p className="text-sm text-gray-500 italic">
                        Nu ai puncte de loialitate disponibile. Câștigă puncte făcând comenzi!
                    </p>
                )}
            </div>

            <div className="p-6 bg-gray-50 border-t flex flex-col sm:flex-row justify-between items-center gap-4">
                <div className="text-left">
                    {cart.loyaltyPointsDiscount && cart.loyaltyPointsDiscount > 0 ? (
                        <div>
                            <div className="text-sm text-gray-600">
                                Subtotal: <span className="line-through">${(cart.totalAmount + cart.loyaltyPointsDiscount).toFixed(2)}</span>
                            </div>
                            <div className="text-sm text-green-600 mb-1">
                                Discount: -${cart.loyaltyPointsDiscount.toFixed(2)}
                            </div>
                            <div className="text-2xl font-bold text-gray-900">
                                Total: <span className="text-blue-600">${cart.totalAmount.toFixed(2)}</span>
                            </div>
                        </div>
                    ) : (
                        <div className="text-2xl font-bold text-gray-900">
                            Total: <span className="text-blue-600">${cart.totalAmount.toFixed(2)}</span>
                        </div>
                    )}
                </div>
                <button
                    onClick={placeOrder}
                    className="w-full sm:w-auto bg-gradient-to-r from-green-500 to-emerald-600 text-white px-8 py-3 rounded-xl hover:from-green-600 hover:to-emerald-700 font-bold shadow-lg transform hover:scale-105 transition duration-300"
                >
                    Plasează Comanda
                </button>
            </div>
        </div>
      </div>
    </div>
  );
};

export default Cart;
