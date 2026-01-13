import React from 'react';
import { Mail, Shield, Star } from 'lucide-react';
import { UserInfo } from '../../types/profileTypes';

interface ProfileHeaderProps {
    user: UserInfo;
}

const ROLE_COLORS: Record<string, string> = {
    Admin: 'bg-red-100 text-red-700 border-red-300',
    Kitchen: 'bg-yellow-100 text-yellow-700 border-yellow-300',
    Buyer: 'bg-green-100 text-green-700 border-green-300',
};

export const ProfileHeader: React.FC<ProfileHeaderProps> = ({ user }) => {
    const userInitial = user.username.charAt(0).toUpperCase();

    return (
        <div className="bg-gradient-to-r from-blue-600 to-purple-600 text-white p-10 rounded-2xl shadow-2xl flex items-center gap-6 animate-fade-in transform transition-all duration-300 hover:shadow-3xl">
            <div className="w-24 h-24 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center text-5xl font-bold border-4 border-white/30">
                {userInitial}
            </div>
            <div>
                <h1 className="text-4xl md:text-5xl font-bold drop-shadow-lg">{user.username}</h1>
                <p className="text-blue-100 text-lg">Profilul utilizatorului</p>
            </div>
        </div>
    );
};

interface UserInfoCardProps {
    user: UserInfo;
    loyaltyPoints: number | null;
}

export const UserInfoCard: React.FC<UserInfoCardProps> = ({ user, loyaltyPoints }) => {
    return (
        <div className="bg-white p-8 rounded-2xl shadow-xl space-y-6 animate-fade-in-delay">
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
                    <span className={`px-3 py-1 rounded-full text-sm border ${ROLE_COLORS[user.role]}`}>
                        {user.role}
                    </span>
                </div>
                {user.role === 'Buyer' && loyaltyPoints !== null && (
                    <div className="flex items-center gap-3 text-gray-700">
                        <Star className="w-5 h-5 text-yellow-500" />
                        <span className="font-medium">Puncte loialitate:</span>
                        {loyaltyPoints}
                    </div>
                )}
            </div>
        </div>
    );
};
