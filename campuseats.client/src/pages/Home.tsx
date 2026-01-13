import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';
import { useLanguage } from '../context/LanguageContext';
import { homeTranslations } from '../i18n/Home';


interface UserInfo {
    username: string;
    role: string;
    loyaltyPoints?: number;
}

const Home: React.FC = () => {
    const { language } = useLanguage();
    const template = homeTranslations[language];
    const { isAuthenticated, isLoading } = useAuth();
    const [user, setUser] = useState<UserInfo | null>(null);
    const [loadingUser, setLoadingUser] = useState(true);

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
                <div role="status" aria-label="loading" className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    if (!isAuthenticated) {
        return (
            <div className="text-center mt-20">
                <h1 className="text-2xl font-bold">{template.authRequiredTitle}</h1>
                <p>
                    <Link to="/login" className="text-blue-500 underline">
                        {template.authRequiredLogin}
                    </Link>{' '}
                    {template.authRequiredOr}{' '}
                    <Link to="/register" className="text-blue-500 underline">
                        {template.authRequiredRegister}
                    </Link>
                </p>
            </div>
        );
    }

    if (!user) {
        return <div className="text-center mt-20">{template.userLoadError}.</div>;
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 page-transition">
            <div className="max-w-7xl mx-auto p-6 space-y-8">
                {/* Welcome banner */}
                <div className="bg-gradient-to-r from-blue-600 to-purple-600 text-white p-8 rounded-2xl shadow-2xl transform transition-all duration-300 hover:shadow-3xl animate-fade-in">
                    <h1 className="text-4xl md:text-5xl font-bold mb-2">
                        {template.welcomeTitle}, {user.username}! 👋
                    </h1>
                    <p className="text-blue-100 text-lg">{template.welcomeSubtitle}</p>
                </div>

                {/* Quick Actions */}
                <div className="grid md:grid-cols-3 gap-6 animate-fade-in-delay">
                    <Link
                        to="/menu"
                        className="bg-white p-6 rounded-xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 border-2 border-transparent hover:border-blue-500"
                    >
                        <div className="text-4xl mb-4">🍽️</div>
                        <h3 className="text-xl font-bold text-gray-900 mb-2">{template.quickActions.menu.title}</h3>
                        <p className="text-gray-600">{template.quickActions.menu.description}</p>
                    </Link>

                    <Link
                        to="/orders"
                        className="bg-white p-6 rounded-xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 border-2 border-transparent hover:border-purple-500"
                    >
                        <div className="text-4xl mb-4">📦</div>
                        <h3 className="text-xl font-bold text-gray-900 mb-2">{template.quickActions.orders.title}</h3>
                        <p className="text-gray-600">{template.quickActions.orders.description}</p>
                    </Link>

                    <Link
                        to="/profile"
                        className="bg-white p-6 rounded-xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 border-2 border-transparent hover:border-green-500"
                    >
                        <div className="text-4xl mb-4">👤</div>
                        <h3 className="text-xl font-bold text-gray-900 mb-2">{template.quickActions.profile.title}</h3>
                        <p className="text-gray-600">{template.quickActions.profile.description}</p>
                    </Link>
                </div>

                {/* Recent activity */}
                <div className="bg-white p-8 rounded-2xl shadow-xl animate-fade-in-delay-2">
                    <h2 className="text-3xl font-bold mb-6 text-gray-900">{template.recentActivity.title}</h2>
                    <div className="bg-gradient-to-r from-blue-50 to-purple-50 p-6 rounded-xl border-l-4 border-blue-500">
                        <p className="text-gray-700 text-lg">
                            {template.recentActivity.description}.
                        </p>
                        <p className="text-gray-500 mt-2">{template.recentActivity.hint}!</p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Home;