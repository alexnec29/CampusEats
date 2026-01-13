import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../hooks/useCart';
import { LoadingSpinner } from '../components/common/LoadingSpinner';

const Cart: React.FC = () => {
  const { cart, loading, hasItems, updateQuantity, removeItem, placeOrder } = useCart();
  const navigate = useNavigate();

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 flex justify-center items-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  if (!hasItems || !cart) {
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

            <div className="p-6 bg-gray-50 border-t flex flex-col sm:flex-row justify-between items-center gap-4">
                <div className="text-2xl font-bold text-gray-900">
                    Total: <span className="text-blue-600">${cart.totalAmount.toFixed(2)}</span>
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
