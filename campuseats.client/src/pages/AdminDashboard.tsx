import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const AdminDashboard: React.FC = () => {
    const navigate = useNavigate();
    const { userRole } = useAuth();

    if (userRole !== 'Admin') {
        return (
            <div className="p-8 text-center">
                <h2 className="text-2xl font-bold">Access Denied</h2>
            </div>
        );
    }

    return (
        <div className="p-8">
            <h2 className="text-3xl font-bold mb-8">Admin Dashboard</h2>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">

                {/* Users */}
                <div className="p-6 bg-white shadow rounded-lg border-l-4 border-blue-500">
                    <h3 className="text-xl font-semibold mb-4">User Management</h3>
                    <p className="text-gray-600 mb-4">View users, change roles, or disable accounts.</p>
                    <button
                        onClick={() => navigate('/admin/users')}
                        className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 w-full"
                    >
                        Manage Users
                    </button>
                </div>

                {/* Menu */}
                <div className="p-6 bg-white shadow rounded-lg border-l-4 border-green-500">
                    <h3 className="text-xl font-semibold mb-4">Menu Management</h3>
                    <p className="text-gray-600 mb-4">Add, edit, or delete menu items.</p>
                    <button
                        onClick={() => navigate('/menu')}
                        className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 w-full"
                    >
                        Manage Menu
                    </button>
                </div>

                {/* Orders */}
                <div className="p-6 bg-white shadow rounded-lg border-l-4 border-purple-500">
                    <h3 className="text-xl font-semibold mb-4">Order Overview</h3>
                    <p className="text-gray-600 mb-4">Monitor all orders placed by users.</p>
                    <button
                        onClick={() => navigate('/orders')}
                        className="px-4 py-2 bg-purple-600 text-white rounded hover:bg-purple-700 w-full"
                    >
                        View Orders
                    </button>
                </div>

            </div>
        </div>
    );
};

export default AdminDashboard;
