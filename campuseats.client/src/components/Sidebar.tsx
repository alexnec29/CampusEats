import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getCsrfToken } from '../utils/csrf';

const Sidebar: React.FC = () => {
  const { isAuthenticated, checkAuthStatus } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      const csrfToken = getCsrfToken();
      const headers: HeadersInit = {};
      if (csrfToken) {
        headers['X-CSRF-TOKEN'] = csrfToken;
      }

      await fetch('/api/user/logout', { 
        method: 'POST',
        headers: headers,
        credentials: 'include'
      });
      await checkAuthStatus();
      navigate('/login');
    } catch (error) {
      console.error('Logout failed', error);
    }
  };

  return (
    <aside className="w-64 bg-gray-800 text-white min-h-screen p-4">
      <nav>
        <ul className="space-y-4">
          <li>
            <Link to="/" className="block hover:text-gray-300">Home</Link>
          </li>
          {!isAuthenticated ? (
            <>
              <li>
                <Link to="/login" className="block hover:text-gray-300">Login</Link>
              </li>
              <li>
                <Link to="/register" className="block hover:text-gray-300">Register</Link>
              </li>
            </>
          ) : (
            <li>
              <button onClick={handleLogout} className="block hover:text-gray-300 text-left w-full">
                Logout
              </button>
            </li>
          )}
        </ul>
      </nav>
    </aside>
  );
};

export default Sidebar;
