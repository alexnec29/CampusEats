import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { User, Mail, Shield, Star, KeyRound, Trash2, Edit } from 'lucide-react';

interface UserInfo {
    username: string;
    role: string;
    email?: string;
    loyaltyPoints?: number;
}

const Profile: React.FC = () => {
    const [user, setUser] = useState<UserInfo | null>(null);

    useEffect(() => {
        const loadUser = async () => {
            const res = await apiClient('/api/user/check-auth');
            const data = await res.json();
            setUser(data);
        };
        loadUser();
    }, []);

    if (!user)
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-500"></div>
            </div>
        );

    const roleColors: Record<string, string> = {
        Admin: "bg-red-100 text-red-700 border-red-300",
        Kitchen: "bg-yellow-100 text-yellow-700 border-yellow-300",
        Buyer: "bg-green-100 text-green-700 border-green-300",
    };

    // Placeholder onclick handlers
    const handleChangePassword = () => {
        alert("Feature: Change password — urmează implementarea backend.");
    };

    const handleChangeEmail = () => {
        alert("Feature: Change email — urmează implementarea backend.");
    };

    const handleDeleteAccount = () => {
        if (window.confirm("Sigur vrei să îți ștergi contul? Această acțiune este ireversibilă.")) {
            alert("Feature: Delete account — backend încă nu este implementat.");
        }
    };

    return (
        <div className="max-w-3xl mx-auto space-y-8">

            {/* Header */}
            <div className="bg-blue-600 text-white p-8 rounded-xl shadow-md flex items-center gap-6">
                <div className="w-20 h-20 rounded-full bg-white/20 flex items-center justify-center text-4xl font-bold">
                    {user.username.charAt(0).toUpperCase()}
                </div>

                <div>
                    <h1 className="text-3xl font-bold">{user.username}</h1>
                    <p className="text-blue-100">Profilul utilizatorului</p>
                </div>
            </div>

            {/* Info Card */}
            <div className="bg-white p-8 rounded-xl shadow space-y-6">
                <h2 className="text-xl font-semibold border-b pb-2">Informații generale</h2>

                <div className="space-y-4">

                    {user.email && (
                        <div className="flex items-center gap-3 text-gray-700">
                            <Mail className="w-5 h-5 text-gray-500" />
                            <span className="font-medium">Email:</span> {user.email}
                        </div>
                    )}

                    <div className="flex items-center gap-3 text-gray-700">
                        <Shield className="w-5 h-5 text-gray-500" />
                        <span className="font-medium">Rol:</span>
                        <span className={`px-3 py-1 rounded-full text-sm border ${roleColors[user.role]}`}>
                            {user.role}
                        </span>
                    </div>

                    {user.loyaltyPoints !== undefined && (
                        <div className="flex items-center gap-3 text-gray-700">
                            <Star className="w-5 h-5 text-yellow-500" />
                            <span className="font-medium">Puncte loialitate:</span>
                            {user.loyaltyPoints}
                        </div>
                    )}
                </div>
            </div>

            {/* Account Settings */}
            <div className="bg-white p-8 rounded-xl shadow space-y-6">
                <h2 className="text-xl font-semibold border-b pb-2">Setări cont</h2>

                <div className="flex flex-col gap-4">

                    <button
                        onClick={handleChangePassword}
                        className="flex items-center justify-between px-5 py-3 rounded-lg bg-gray-100 hover:bg-gray-200 transition shadow-sm"
                    >
                        <span className="flex items-center gap-3 font-medium">
                            <KeyRound className="w-5 h-5" />
                            Schimbă parola
                        </span>
                        <Edit className="w-5 h-5 text-gray-500" />
                    </button>

                    <button
                        onClick={handleChangeEmail}
                        className="flex items-center justify-between px-5 py-3 rounded-lg bg-gray-100 hover:bg-gray-200 transition shadow-sm"
                    >
                        <span className="flex items-center gap-3 font-medium">
                            <Mail className="w-5 h-5" />
                            Schimbă emailul
                        </span>
                        <Edit className="w-5 h-5 text-gray-500" />
                    </button>

                    <button
                        onClick={handleDeleteAccount}
                        className="flex items-center justify-between px-5 py-3 rounded-lg bg-red-100 hover:bg-red-200 transition shadow-sm text-red-700"
                    >
                        <span className="flex items-center gap-3 font-semibold">
                            <Trash2 className="w-5 h-5" />
                            Șterge contul
                        </span>
                    </button>

                </div>
            </div>
        </div>
    );
};

export default Profile;
