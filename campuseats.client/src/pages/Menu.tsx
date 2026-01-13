import React from 'react';
import { useMenu } from '../hooks/useMenu';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { MenuItem } from '../types';

const Menu: React.FC = () => {
  const { menuItems, loading, userRole, addToOrder, handleDelete, navigate } = useMenu();

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 flex justify-center items-center">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  const isBuyer = userRole === 'Buyer';
  const isKitchenOrAdmin = userRole === 'Kitchen' || userRole === 'Admin';

  const renderMenuItem = (item: MenuItem) => {
    const isAvailable = item.isAvailable;
    const buttonText = isAvailable ? 'Adaugă în Coș' : 'Indisponibil';
    const buttonClasses = isAvailable
      ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white hover:from-blue-700 hover:to-purple-700 hover:shadow-lg'
      : 'bg-gray-300 text-gray-500 cursor-not-allowed';

    return (
      <div key={item.id} className="bg-white rounded-2xl shadow-lg overflow-hidden transform transition-all duration-300 hover:shadow-2xl hover:-translate-y-2">
        <div className="relative h-48 bg-gray-200">
          {item.imageUrl ? (
            <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-gray-400">
              <span className="text-4xl">🍽️</span>
            </div>
          )}
          {!isAvailable && (
            <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center">
              <span className="bg-red-500 text-white px-4 py-1 rounded-full font-bold transform -rotate-12">
                Indisponibil
              </span>
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
            {isBuyer && (
              <button
                onClick={() => addToOrder(item)}
                className={`flex-1 py-3 px-4 rounded-xl font-bold shadow-md transition-all duration-300 transform active:scale-95 ${buttonClasses}`}
                disabled={!isAvailable}
              >
                {buttonText}
              </button>
            )}

            {isKitchenOrAdmin && (
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
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 p-6 page-transition">
      <div className="max-w-7xl mx-auto">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h2 className="text-4xl font-bold text-gray-900 mb-2">Meniu</h2>
            <p className="text-gray-600">
              {isBuyer ? 'Descoperă preparatele noastre delicioase' : 'Modify menu items'}
            </p>
          </div>
          {isBuyer && (
            <button
              onClick={() => navigate('/cart')}
              className="bg-gradient-to-r from-green-500 to-emerald-600 text-white px-6 py-3 rounded-xl hover:from-green-600 hover:to-emerald-700 font-bold shadow-lg transform hover:scale-105 transition duration-300 flex items-center"
            >
              <span className="mr-2">🛒</span> Vezi Coșul
            </button>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {menuItems.map(renderMenuItem)}
        </div>
      </div>
    </div>
  );
};

export default Menu;
