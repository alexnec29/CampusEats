import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';

interface UserInfo {
    username: string;
    role: string;
    loyaltyPoints?: number;
}

const Home: React.FC = () => {
    const { isAuthenticated, isLoading } = useAuth();
    const [user, setUser] = useState<UserInfo | null>(null);
    const [loadingUser, setLoadingUser] = useState(true);

    // Fetch user info only if authenticated
    useEffect(() => {
        if (!isAuthenticated) {
            setLoadingUser(false);
            return;
        }

        const fetchUser = async () => {
            try {
                const res = await apiClient('/api/user/check-auth');
                if (res.ok) {
                    const data = await res.json();
                    setUser(data);
                } else {
                    setUser(null);
                }
            } catch {
                setUser(null);
            } finally {
                setLoadingUser(false);
            }
        };

        fetchUser();
    }, [isAuthenticated]);

    if (isLoading || loadingUser) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    if (!isAuthenticated) {
        return (
            <div className="text-center mt-20">
                <h1 className="text-2xl font-bold">Trebuie să te loghezi</h1>
                <p>
                    <Link to="/login" className="text-blue-500 underline">
                        Login
                    </Link>{' '}
                    sau{' '}
                    <Link to="/register" className="text-blue-500 underline">
                        Register
                    </Link>
                </p>
            </div>
        );
    }

    if (!user) {
        return <div className="text-center mt-20">Nu am putut încărca datele utilizatorului.</div>;
    }

    return (
        <div className="space-y-8">
            {/* Welcome banner */}
            <div className="bg-blue-500 text-white p-6 rounded-lg shadow-md">
                <h1 className="text-3xl font-bold">Bine ai venit, {user.username}!</h1>
                <p className="mt-2">Rolul tău: <span className="font-semibold">{user.role}</span></p>
                {user.loyaltyPoints !== undefined && (
                    <p>Puncte loialitate: <span className="font-semibold">{user.loyaltyPoints}</span></p>
                )}
            </div>

            {/* Quick links */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <Link
                    to="/menu"
                    className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition duration-200 flex flex-col items-center justify-center"
                >
                    <span className="text-xl font-bold">🍽️ Menu</span>
                    <p className="mt-2 text-gray-600">Vezi meniul disponibil</p>
                </Link>

                <Link
                    to="/orders"
                    className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition duration-200 flex flex-col items-center justify-center"
                >
                    <span className="text-xl font-bold">🛒 Comenzi</span>
                    <p className="mt-2 text-gray-600">Vezi comenzile tale</p>
                </Link>

                <Link
                    to="/loyalty"
                    className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition duration-200 flex flex-col items-center justify-center"
                >
                    <span className="text-xl font-bold">🎁 Loialitate</span>
                    <p className="mt-2 text-gray-600">Răsfață-te cu punctele acumulate</p>
                </Link>

                <Link
                    to="/profile"
                    className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition duration-200 flex flex-col items-center justify-center"
                >
                    <span className="text-xl font-bold">👤 Profil</span>
                    <p className="mt-2 text-gray-600">Vezi și modifică datele tale</p>
                </Link>

                {/* Role-specific card */}
                {user.role === 'Kitchen' && (
                    <Link
                        to="/kitchen"
                        className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition duration-200 flex flex-col items-center justify-center"
                    >
                        <span className="text-xl font-bold">👨‍🍳 Bucătărie</span>
                        <p className="mt-2 text-gray-600">Gestionați comenzile și stocul</p>
                    </Link>
                )}
            </div>

            {/* Recent activity placeholder */}
            <div className="bg-white p-6 rounded-lg shadow-md">
                <h2 className="text-2xl font-bold mb-4">Activitate recentă</h2>
                <p className="text-gray-600">Ultimele comenzi sau puncte câștigate vor apărea aici.</p>
            </div>
        </div>
    );
};

export default Home;
