import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';

const Sidebar: React.FC = () => {
  const { isAuthenticated, userRole, checkAuthStatus } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await apiClient('/api/user/logout', { method: 'POST' });
      await checkAuthStatus();
      navigate('/login');
    } catch (error) {
      console.error('Logout failed', error);
    }
  };

  if (!isAuthenticated) return null;

  return (
      <aside className="w-64 bg-gray-800 text-white min-h-screen p-6 flex flex-col">
        <div className="flex flex-col space-y-8">
          <div className="flex flex-col">
            <h3 className="text-white text-lg font-bold mb-4 uppercase">Navigare</h3>
            <ul className="flex flex-col space-y-2">
              <li>
                <Link to="/" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  Home
                </Link>
              </li>
              <li>
                <Link to="/menu" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  Menu
                </Link>
              </li>
              {(userRole === 'Buyer' || userRole === 'Admin') && (
              <li>
                <Link to="/orders" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  Comenzi
                </Link>
              </li>
              )}
            </ul>
          </div>

          <hr className="border-gray-600 my-4" />

          {(userRole === 'Kitchen' || userRole === 'Admin') && (
          <>
          <div className="flex flex-col">
            <h3 className="text-white text-lg font-bold mb-4 uppercase">Administrare</h3>
            <ul className="flex flex-col space-y-2">
              <li>
                <Link to="/kitchen-orders" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  Kitchen Dashboard
                </Link>
              </li>
              <li>
                <Link to="/add-menu-item" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  Adaugă produs
                </Link>
              </li>
            </ul>
          </div>

          <hr className="border-gray-600 my-4" />
          </>
          )}

          <div className="flex flex-col">
            <h3 className="text-white text-lg font-bold mb-4 uppercase">Contul meu</h3>
            <ul className="flex flex-col space-y-2">
              <li>
                <Link to="/profile" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  Profil
                </Link>
              </li>
              <li>
                <button
                    onClick={handleLogout}
                    className="block w-full text-left px-4 py-2 rounded hover:bg-gray-700 transition"
                >
                  Logout
                </button>
              </li>
            </ul>
          </div>
        </div>
      </aside>
  );
};

export default Sidebar;
