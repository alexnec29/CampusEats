import React from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';
import { 
  Home, 
  UtensilsCrossed, 
  ShoppingBag, 
  ChefHat, 
  PlusCircle, 
  LayoutDashboard, 
  User, 
  LogOut 
} from 'lucide-react';

const LinkItem = ({ to, icon: Icon, label, active }: { to: string; icon: React.ElementType; label: string; active: boolean }) => {
  return (
    <li>
      <Link 
        to={to} 
        className={`flex items-center space-x-3 px-4 py-2.5 rounded-lg transition-all duration-200 group ${
          active 
            ? 'bg-blue-50 text-blue-600 font-medium shadow-sm' 
            : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
        }`}
      >
        <Icon className={`w-5 h-5 ${active ? 'text-blue-600' : 'text-gray-400 group-hover:text-gray-600'}`} />
        <span>{label}</span>
      </Link>
    </li>
  );
};

const Sidebar: React.FC = () => {
  const { isAuthenticated, userRole, checkAuthStatus } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

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

  const isActive = (path: string) => location.pathname === path;

  return (
      <aside className="w-64 bg-white border-r border-gray-200 min-h-screen p-6 flex flex-col shadow-sm">
        <div className="flex flex-col space-y-8">
          <div className="flex flex-col">
            <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4 px-4">
              Navigare
            </h3>
            <ul className="flex flex-col space-y-1">
              <LinkItem to="/home" icon={Home} label="Home" active={isActive('/home')} />
              <LinkItem to="/menu" icon={UtensilsCrossed} label="Meniu" active={isActive('/menu')} />
              {(userRole === 'Buyer' || userRole === 'Admin') && (
                <LinkItem to="/orders" icon={ShoppingBag} label="Comenzi" active={isActive('/orders')} />
              )}
            </ul>
          </div>

          {(userRole === 'Kitchen' || userRole === 'Admin') && (
              <>
                <div className="border-t border-gray-100 my-2"></div>
                <div className="flex flex-col">
                  <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4 px-4 mt-2">
                    Administrare
                  </h3>
                  <ul className="flex flex-col space-y-1">
                    <LinkItem to="/kitchen-orders" icon={ChefHat} label="Kitchen Dashboard" active={isActive('/kitchen-orders')} />
                    <LinkItem to="/add-menu-item" icon={PlusCircle} label="Adaugă produs" active={isActive('/add-menu-item')} />

                    {userRole === 'Admin' && (
                      <LinkItem to="/admin" icon={LayoutDashboard} label="Admin Dashboard" active={isActive('/admin')} />
                    )}
                  </ul>
                </div>
              </>
          )}

          <div className="border-t border-gray-100 my-2"></div>

          <div className="flex flex-col mt-auto">
            <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4 px-4 mt-2">
              Contul meu
            </h3>
            <ul className="flex flex-col space-y-1">
              <LinkItem to="/profile" icon={User} label="Profil" active={isActive('/profile')} />
              <li>
                <button
                    onClick={handleLogout}
                    className="w-full flex items-center space-x-3 px-4 py-2.5 rounded-lg transition-all duration-200 group text-gray-600 hover:bg-red-50 hover:text-red-600"
                >
                  <LogOut className="w-5 h-5 text-gray-400 group-hover:text-red-500" />
                  <span>Logout</span>
                </button>
              </li>
            </ul>
          </div>
        </div>
      </aside>
  );
};

export default Sidebar;
