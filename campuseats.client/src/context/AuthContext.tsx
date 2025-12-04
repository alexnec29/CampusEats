import React, { createContext, useState, useEffect, useContext, ReactNode } from 'react';
import { apiClient } from '../utils/apiClient';

interface AuthContextType {
  isAuthenticated: boolean;
  userRole: string | null;
  isLoading: boolean;
  checkAuthStatus: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [userRole, setUserRole] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const checkAuthStatus = async () => {
    try {
      const response = await apiClient('/api/user/check-auth');
      if (response.ok) {
        const data = await response.json();
        setIsAuthenticated(data.isAuthenticated);
        setUserRole(data.role || null);
      } else {
        setIsAuthenticated(false);
        setUserRole(null);
      }
    } catch (error) {
      setIsAuthenticated(false);
      setUserRole(null);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    checkAuthStatus();
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthenticated, userRole, isLoading, checkAuthStatus }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
