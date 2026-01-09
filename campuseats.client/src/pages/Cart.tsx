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
  const [applyingDiscount, setApplyingDiscount] = useState(false);
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
        setLoyaltyPoints(data.pointsBalance || 0);
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

  const applyLoyaltyDiscount = async () => {
    if (!cart) return;
    
    if (pointsToRedeem <= 0) {
      showToast('Te rog introdu puncte de redeem', 'error');
      return;
    }

    if (pointsToRedeem > loyaltyPoints) {
      showToast('Nu ai suficiente puncte de loialitate', 'error');
      return;
    }

    setApplyingDiscount(true);
    try {
      const response = await apiClient(`/api/orders/${cart.id}/apply-loyalty-discount`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ pointsToRedeem: pointsToRedeem })
      });

      if (response.ok) {
        const data = await response.json();
        showToast(`Discount aplicat! Economisești $${data.discountAmount.toFixed(2)}`, 'success');
        setLoyaltyPoints(data.remainingPoints);
        setPointsToRedeem(0);
        fetchCart(); // Refresh cart to show updated total
      } else {
        const errorData = await response.json();
        showToast(errorData.message || 'Nu s-a putut aplica discountul', 'error');
      }
    } catch (error) {
      console.error('Error applying discount:', error);
      showToast('Eroare la aplicarea discountului', 'error');
    } finally {
      setApplyingDiscount(false);
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
            <div className="p-6 bg-blue-50 border-t border-blue-100">
                <div className="mb-4">
                    <h3 className="text-lg font-semibold text-gray-800 mb-2">🎁 Puncte de Loialitate</h3>
                    <p className="text-sm text-gray-600 mb-3">
                        Ai <span className="font-bold text-blue-600">{loyaltyPoints}</span> puncte disponibile
                        <span className="text-xs ml-2">(100 puncte = $1 discount)</span>
                    </p>
                </div>
                
                {cart.loyaltyPointsUsed && cart.loyaltyPointsUsed > 0 ? (
                    <div className="bg-green-100 border border-green-300 rounded-lg p-4">
                        <p className="text-green-800 font-semibold">
                            ✓ Discount aplicat: {cart.loyaltyPointsUsed} puncte → -${cart.discountAmount?.toFixed(2)}
                        </p>
                    </div>
                ) : (
                    <div className="flex flex-col sm:flex-row gap-3">
                        <input
                            type="number"
                            min="0"
                            max={loyaltyPoints}
                            value={pointsToRedeem}
                            onChange={(e) => {
                                const value = Number(e.target.value);
                                if (Number.isInteger(value) && value >= 0) {
                                    setPointsToRedeem(Math.max(0, Math.min(loyaltyPoints, value)));
                                } else if (e.target.value === '') {
                                    setPointsToRedeem(0);
                                }
                            }}
                            placeholder="Puncte de folosit"
                            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                            disabled={loyaltyPoints === 0 || applyingDiscount}
                        />
                        <button
                            onClick={applyLoyaltyDiscount}
                            disabled={pointsToRedeem <= 0 || applyingDiscount || loyaltyPoints === 0}
                            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed font-semibold transition-colors"
                        >
                            {applyingDiscount ? 'Se aplică...' : 'Aplică Discount'}
                        </button>
                    </div>
                )}
            </div>

            <div className="p-6 bg-gray-50 border-t flex flex-col sm:flex-row justify-between items-center gap-4">
                <div className="text-left">
                    {cart.loyaltyPointsUsed && cart.loyaltyPointsUsed > 0 && cart.discountAmount ? (
                        <div>
                            <div className="text-sm text-gray-500 line-through">
                                Original: ${(cart.totalAmount + cart.discountAmount).toFixed(2)}
                            </div>
                            <div className="text-2xl font-bold text-gray-900">
                                Total: <span className="text-green-600">${cart.totalAmount.toFixed(2)}</span>
                            </div>
                            <div className="text-xs text-green-600">
                                Ai economisit ${cart.discountAmount.toFixed(2)}!
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
