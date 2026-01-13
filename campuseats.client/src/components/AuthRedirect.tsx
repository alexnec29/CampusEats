// src/components/AuthRedirect.tsx
import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface AuthRedirectProps {
    children: React.ReactNode;
    redirectTo: string;
}

const AuthRedirect: React.FC<AuthRedirectProps> = ({ children, redirectTo }) => {
    const { isAuthenticated, isLoading } = useAuth();

    if (isLoading) return <div>Loading...</div>;

    // FIX: Dacă ESTE autentificat, îl redirecționăm (ex: de la Login la Home)
    if (isAuthenticated) return <Navigate to={redirectTo} replace />;

    return <>{children}</>;
};

export default AuthRedirect;
