import React, { ReactNode } from 'react';
import Header from './Header';
import Sidebar from './Sidebar';
import { useAuth } from '../context/AuthContext';

interface LayoutProps {
    children: ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
    const { isAuthenticated } = useAuth();

    return (
        <div className="flex min-h-screen">
            {isAuthenticated && <Sidebar />}

            <div className="flex-1 flex flex-col">
                {isAuthenticated && <Header />}
                <main className={`flex-1 ${isAuthenticated ? 'p-8' : ''}`}>{children}</main>
            </div>
        </div>
    );
};

export default Layout;
