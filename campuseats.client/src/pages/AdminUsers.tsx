import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';

interface AppUser {
    id: string;
    username: string;
    email: string;
    role: string;
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
        } finally {
            setLoading(false);
        }
    };

    const updateRole = async (userId: string, role: string) => {
        try {
            const res = await apiClient(`/api/admin/users/${userId}/role`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ role }),
            });
            if (res.ok) fetchUsers();
        } catch (err) {
            console.error('Error updating role', err);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    if (loading)
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
        );

    return (
        <div className="p-8">
            <h2 className="text-3xl font-bold mb-6 text-gray-800">Manage Users</h2>

            <div className="overflow-x-auto bg-white shadow rounded-lg">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                    <tr>
                        <th className="px-6 py-3 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Username</th>
                        <th className="px-6 py-3 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Email</th>
                        <th className="px-6 py-3 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Role</th>
                    </tr>
                    </thead>

                    <tbody className="bg-white divide-y divide-gray-200">
                    {users.map((u) => (
                        <tr key={u.id}>
                            <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{u.username}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{u.email}</td>

                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                <select
                                    value={u.role}
                                    onChange={(e) => updateRole(u.id, e.target.value)}
                                    className="border rounded px-2 py-1 text-sm"
                                >
                                    <option value="Buyer">Buyer</option>
                                    <option value="Kitchen">Kitchen</option>
                                    <option value="Admin">Admin</option>
                                </select>
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
