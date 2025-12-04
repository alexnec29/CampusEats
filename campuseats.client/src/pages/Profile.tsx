import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { useAuth } from '../context/AuthContext';

interface ProfileData {
    username: string;
    email?: string;
    role: string;
    loyaltyPoints?: number;
    kitchenName?: string;
}

const Profile: React.FC = () => {
    const { isAuthenticated, isLoading } = useAuth();
    const [profile, setProfile] = useState<ProfileData | null>(null);
    const [loadingProfile, setLoadingProfile] = useState(true);

    useEffect(() => {
        if (!isAuthenticated) {
            setLoadingProfile(false);
            return;
        }

        const fetchProfile = async () => {
            try {
                const res = await apiClient('/api/user', { credentials: 'include' });
                if (!res.ok) throw new Error('Failed to fetch profile');
                const data = await res.json();
                setProfile(data);
            } catch (err) {
                console.error(err);
                setProfile(null);
            } finally {
                setLoadingProfile(false);
            }
        };

        fetchProfile();
    }, [isAuthenticated]);

    if (isLoading || loadingProfile) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    if (!isAuthenticated) {
        return (
            <div className="text-center mt-20">
                <h1 className="text-3xl font-bold mb-2">Trebuie să te loghezi</h1>
                <p className="text-gray-600">Login sau register pentru a vedea profilul</p>
            </div>
        );
    }

    if (!profile) {
        return <div className="text-center mt-20 text-red-500">Nu am putut încărca datele profilului.</div>;
    }

    return (
        <div className="max-w-2xl mx-auto p-6 bg-white rounded-xl shadow-lg border border-gray-200 space-y-6">
            <h1 className="text-3xl font-extrabold text-gray-800">Profilul tău</h1>

            <div className="space-y-3">
                <div className="flex justify-between items-center">
                    <span className="font-semibold text-gray-600">Username:</span>
                    <span className="text-gray-800">{profile.username}</span>
                </div>

                <div className="flex justify-between items-center">
                    <span className="font-semibold text-gray-600">Email:</span>
                    <span className="text-gray-800">{profile.email}</span>
                </div>

                <div className="flex justify-between items-center">
                    <span className="font-semibold text-gray-600">Rol:</span>
                    <span className="text-gray-800">{profile.role}</span>
                </div>

                {profile.role === 'Buyer' && (
                    <div className="flex justify-between items-center">
                        <span className="font-semibold text-gray-600">Puncte loialitate:</span>
                        <span className="text-gray-800">{profile.loyaltyPoints ?? 0}</span>
                    </div>
                )}

                {profile.role === 'Kitchen' && (
                    <div className="flex justify-between items-center">
                        <span className="font-semibold text-gray-600">Bucătărie:</span>
                        <span className="text-gray-800">{profile.kitchenName ?? "N/A"}</span>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Profile;
