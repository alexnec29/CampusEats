// src/components/PublicRoute.tsx
import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface PublicRouteProps {
    children: React.ReactNode;
}

const PublicRoute: React.FC<PublicRouteProps> = ({ children }) => {
    const { isAuthenticated, isLoading } = useAuth();

    if (isLoading) return <div>Loading...</div>;

    if (isAuthenticated) return <Navigate to="/home" replace />;

    return <>{children}</>;
};

export default PublicRoute;
