import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { useToast } from '../context/ToastContext';

interface AppUser {
    id: string;
    username: string;
    email: string;
    role: string;
    loyaltyPoints?: number;
}

const AdminUsers: React.FC = () => {
    const [users, setUsers] = useState<AppUser[]>([]);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();

    const [selectedUser, setSelectedUser] = useState<AppUser | null>(null);
    const [delta, setDelta] = useState<number>(0);
    const [saving, setSaving] = useState(false);

    const fetchUsers = async () => {
        try {
            const res = await apiClient('/api/admin/users');
            if (res.ok) {
                const data = await res.json();
                setUsers(data);
            }
        } catch (err) {
            console.error('Error loading users', err);
        } finally {
            setLoading(false);
        }
    };

    const updateRole = async (userId: string, role: string) => {
        await apiClient(`/api/admin/users/${userId}/role`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ role }),
        });
        fetchUsers();
    };

    const applyAdjustment = async () => {
        if (!selectedUser || delta === 0) return;

        setSaving(true);

        try {
            const res = await apiClient('/api/loyalty/adjust', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    userId: selectedUser.id,
                    points: delta,
                    reason: 'Admin manual adjustment',
                }),
            });

            if (res.ok) {
                const data = await res.json();

                setUsers(prev =>
                    prev.map(u =>
                        u.id === selectedUser.id
                            ? { ...u, loyaltyPoints: data.pointsBalance }
                            : u
                    )
                );

                setSelectedUser(prev =>
                    prev ? { ...prev, loyaltyPoints: data.pointsBalance } : prev
                );

                setDelta(0);
                showToast('Puncte actualizate cu succes', 'success');
            } else {
                const errorText = await res.text();
                showToast(errorText || 'Eroare la actualizarea punctelor', 'error');
            }
        } catch (err) {
            console.error('Error adjusting points', err);
            showToast('Eroare la actualizarea punctelor', 'error');
        }

        setSaving(false);
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    if (loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <div
                    role="status"
                    aria-label="loading"
                    className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"
                />
            </div>
        );
    }

    return (
        <div className="p-8">
            <h2 className="text-3xl font-bold mb-6">Manage Users</h2>

            <table className="min-w-full bg-white shadow rounded">
                <thead className="bg-gray-50">
                <tr>
                    <th className="p-3 text-left">Username</th>
                    <th className="p-3 text-left">Email</th>
                    <th className="p-3 text-left">Role</th>
                    <th className="p-3 text-left">Loyalty</th>
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
                                onChange={(e) => updateRole(u.id, e.target.value)}
                                className="border px-2 py-1 rounded"
                            >
                                <option value="Buyer">Buyer</option>
                                <option value="Kitchen">Kitchen</option>
                                <option value="Admin">Admin</option>
                            </select>
                        </td>
                        <td className="p-3">
                            {u.role === 'Buyer' ? (
                                <button
                                    onClick={() => {
                                        setSelectedUser(u);
                                        setDelta(0);
                                    }}
                                    className="text-blue-600 underline text-sm"
                                >
                                    Loyalty
                                </button>
                            ) : (
                                '-'
                            )}
                        </td>
                    </tr>
                ))}
                </tbody>
            </table>

            {selectedUser && (
                <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center">
                    <div className="bg-white rounded-lg p-6 w-96">
                        <h3 className="text-xl font-semibold mb-4">
                            Loyalty – {selectedUser.username}
                        </h3>

                        <p className="mb-3">
                            Current points:{' '}
                            <strong>{selectedUser.loyaltyPoints ?? 0}</strong>
                        </p>

                        <input
                            type="number"
                            placeholder="+ / -"
                            value={delta}
                            onChange={(e) => setDelta(parseInt(e.target.value) || 0)}
                            className="border px-3 py-1 w-full mb-4"
                        />

                        <div className="flex justify-end gap-2">
                            <button
                                onClick={() => setSelectedUser(null)}
                                className="px-4 py-1 border rounded"
                                disabled={saving}
                            >
                                Cancel
                            </button>
                            <button
                                onClick={applyAdjustment}
                                className="px-4 py-1 bg-blue-600 text-white rounded"
                                disabled={saving}
                            >
                                Apply
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default AdminUsers;