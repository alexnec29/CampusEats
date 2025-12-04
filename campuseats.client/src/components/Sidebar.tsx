import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';

const Sidebar: React.FC = () => {
  const { isAuthenticated, checkAuthStatus } = useAuth();
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
      <aside className="w-64 bg-gray-800 text-white min-h-screen p-4">
        <nav>
          <ul className="space-y-4">
            <li>
              <Link to="/" className="block hover:text-gray-300">Home</Link>
            </li>
            <li>
              <Link to="/menu" className="block hover:text-gray-300">Menu</Link>
            </li>
            <li>
              <Link to="/orders" className="block hover:text-gray-300">Comenzi</Link>
            </li>
            <li>
              <Link to="/profile" className="block hover:text-gray-300">Profil</Link>
            </li>
            {/* Only kitchen/admin users can add menu items */}
            <li>
              <Link to="/add-menu-item" className="block hover:text-gray-300">Adaugă produs</Link>
            </li>

            <li>
              <button
                  onClick={handleLogout}
                  className="block hover:text-gray-300 text-left w-full"
              >
                Logout
              </button>
            </li>
          </ul>
        </nav>
      </aside>
  );
};

export default Sidebar;
