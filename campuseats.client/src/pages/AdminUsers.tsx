import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';

interface AppUser {
    id: string;
    username: string;
    email: string;
    role: string;
    isActive: boolean;
}

const AdminUsers: React.FC = () => {
    const [users, setUsers] = useState<AppUser[]>([]);
    const [loading, setLoading] = useState(true);

    const fetchUsers = async () => {
        try {
            const res = await apiClient('/api/admin/users');
            if (res.ok) {
                setUsers(await res.json());
            }
        } catch (err) {
            console.error('Error loading users', err);
        }
        setLoading(false);
    };

    const updateRole = async (userId: string, role: string) => {
        const res = await apiClient(`/api/admin/users/${userId}/role`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ role })
        });

        if (res.ok) fetchUsers();
    };

    const toggleActive = async (userId: string) => {
        const res = await apiClient(`/api/admin/users/${userId}/toggle-active`, {
            method: 'PUT'
        });

        if (res.ok) fetchUsers();
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    if (loading) return <div className="p-4">Loading users...</div>;

    return (
        <div className="p-8">
            <h2 className="text-3xl font-bold mb-6">Manage Users</h2>

            <div className="bg-white shadow rounded-lg overflow-hidden">
                <table className="w-full">
                    <thead className="bg-gray-100">
                    <tr>
                        <th className="p-3 text-left">Username</th>
                        <th className="p-3 text-left">Email</th>
                        <th className="p-3 text-left">Role</th>
                        <th className="p-3 text-left">Status</th>
                        <th className="p-3 text-right">Actions</th>
                    </tr>
                    </thead>

                    <tbody>
                    {users.map(u => (
                        <tr key={u.id} className="border-t">
                            <td className="p-3">{u.username}</td>
                            <td className="p-3">{u.email}</td>

                            <td className="p-3">
                                <select
                                    value={u.role}
                                    onChange={e => updateRole(u.id, e.target.value)}
                                    className="border rounded p-1"
                                >
                                    <option value="Buyer">Buyer</option>
                                    <option value="Kitchen">Kitchen</option>
                                    <option value="Admin">Admin</option>
                                </select>
                            </td>

                            <td className="p-3">
                                {u.isActive ? (
                                    <span className="text-green-600 font-semibold">Active</span>
                                ) : (
                                    <span className="text-red-600 font-semibold">Disabled</span>
                                )}
                            </td>

                            <td className="p-3 text-right">
                                <button
                                    onClick={() => toggleActive(u.id)}
                                    className={`px-3 py-1 rounded text-white ${
                                        u.isActive ? 'bg-red-600 hover:bg-red-700' : 'bg-green-600 hover:bg-green-700'
                                    }`}
                                >
                                    {u.isActive ? 'Disable' : 'Enable'}
                                </button>
                            </td>
                        </tr>
                    ))}
                    </tbody>

                </table>
            </div>
        </div>
    );
};

export default AdminUsers;
