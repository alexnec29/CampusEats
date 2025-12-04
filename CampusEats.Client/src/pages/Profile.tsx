import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';

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

    if (!user) return <div>Loading profile...</div>;

    return (
        <div className="space-y-6">
            <h1 className="text-2xl font-bold">Profilul meu</h1>

            <div className="bg-white p-6 rounded-lg shadow-md space-y-2">
                <p><strong>Username:</strong> {user.username}</p>
                {user.email && <p><strong>Email:</strong> {user.email}</p>}
                <p><strong>Rol:</strong> {user.role}</p>
                {user.loyaltyPoints !== undefined && (
                    <p><strong>Puncte loialitate:</strong> {user.loyaltyPoints}</p>
                )}
            </div>
        </div>
    );
};

export default Profile;
