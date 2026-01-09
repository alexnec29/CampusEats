import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';
import { useLanguage } from '../context/LanguageContext';
import { sidebarTranslations } from '../i18n/Sidebar'; 

const Sidebar: React.FC = () => {
  const { language } = useLanguage();
  const template = sidebarTranslations[language] ;
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
            <h3 className="text-white text-lg font-bold mb-4 uppercase">{template.navigationTitle}</h3>
            <ul className="flex flex-col space-y-2">
              <li>
                <Link to="/home" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  {template.links.home}
                </Link>
              </li>
              <li>
                <Link to="/menu" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  {template.links.menu}
                </Link>
              </li>
              {(userRole === 'Buyer' || userRole === 'Admin') && (
              <li>
                <Link to="/orders" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  {template.links.orders}
                </Link>
              </li>
              )}
            </ul>
          </div>

          <hr className="border-gray-600 my-4" />

          {(userRole === 'Kitchen' || userRole === 'Admin') && (
              <>
                <div className="flex flex-col">
                  <h3 className="text-white text-lg font-bold mb-4 uppercase">{template.administration}</h3>
                  <ul className="flex flex-col space-y-2">
                    <li>
                      <Link to="/kitchen-orders" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                        {template.links.kitchenDashboard}
                      </Link>
                    </li>
                    <li>
                      <Link to="/add-menu-item" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                        {template.links.addNewMenuItem}
                      </Link>
                    </li>

                    {userRole === 'Admin' && (
                        <li>
                          <Link to="/admin" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                            {template.links.adminDashboard}
                          </Link>
                        </li>
                    )}
                  </ul>
                </div>

                <hr className="border-gray-600 my-4" />
              </>
          )}

          <div className="flex flex-col">
            <h3 className="text-white text-lg font-bold mb-4 uppercase">{template.myAccount}</h3>
            <ul className="flex flex-col space-y-2">
              <li>
                <Link to="/profile" className="block px-4 py-2 rounded hover:bg-gray-700 transition">
                  {template.links.profile}
                </Link>
              </li>
              <li>
                <button
                    onClick={handleLogout}
                    className="block w-full text-left px-4 py-2 rounded hover:bg-gray-700 transition"
                >
                  {template.links.logout}
                </button>
              </li>
            </ul>
          </div>
        </div>
      </aside>
  );
};

export default Sidebar;
