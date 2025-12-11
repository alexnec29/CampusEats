import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<string>('all');
  const [expandedOrder, setExpandedOrder] = useState<number | null>(null);
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    const fetchOrders = async () => {
      try {
        const response = await apiClient('/api/orders/my-orders');
        if (response.ok) {
          const data: Order[] = await response.json();
          const placedOrders = data.filter(o => o.status !== OrderStatus.Pending);
          const sorted = placedOrders.sort((a: Order, b: Order) => 
            new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()
          );
          setOrders(sorted);
        }
      } catch (error) {
        console.error('Error fetching orders:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchOrders();
  }, [isAuthenticated, navigate]);

  const getStatusLabel = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Inactive: return 'Inactiv';
      case OrderStatus.Pending: return 'Coș';
      case OrderStatus.Placed: return 'Plasat';
      case OrderStatus.Paid: return 'Plătit';
      case OrderStatus.Preparing: return 'Se Prepară';
      case OrderStatus.Ready: return 'Gata';
      case OrderStatus.Completed: return 'Finalizat';
      case OrderStatus.Cancelled: return 'Anulat';
      default: return 'Necunoscut';
    }
  };

  const getStatusIcon = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Placed: return '📋';
      case OrderStatus.Paid: return '💳';
      case OrderStatus.Preparing: return '👨‍🍳';
      case OrderStatus.Ready: return '✅';
      case OrderStatus.Completed: return '🎉';
      case OrderStatus.Cancelled: return '❌';
      default: return '📦';
    }
  };

  const getStatusColor = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Inactive: return 'bg-gray-100 text-gray-700 border-gray-300';
      case OrderStatus.Placed: return 'bg-blue-100 text-blue-700 border-blue-300';
      case OrderStatus.Paid: return 'bg-green-100 text-green-700 border-green-300';
      case OrderStatus.Preparing: return 'bg-purple-100 text-purple-700 border-purple-300';
      case OrderStatus.Ready: return 'bg-yellow-100 text-yellow-700 border-yellow-300';
      case OrderStatus.Completed: return 'bg-emerald-100 text-emerald-700 border-emerald-300';
      case OrderStatus.Cancelled: return 'bg-red-100 text-red-700 border-red-300';
      default: return 'bg-gray-100 text-gray-700';
    }
  };

  const filteredOrders = filter === 'all' 
    ? orders 
    : orders.filter(o => getStatusLabel(o.status).toLowerCase().includes(filter.toLowerCase()));

  const removeItem = async (orderId: number, itemId: number) => {
    if (!window.confirm('Ești sigur că vrei să elimini acest articol?')) return;

    try {
      const response = await apiClient(`/api/orders/${orderId}/items/${itemId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        const ordersRes = await apiClient('/api/orders/my-orders');
        if (ordersRes.ok) {
          const data = await ordersRes.json();
          const sorted = data.sort((a: Order, b: Order) => 
            new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()
          );
          setOrders(sorted);
        }
      } else {
        alert('Eroare la ștergerea articolului');
      }
    } catch (error) {
      console.error('Error removing item:', error);
      alert('Eroare la ștergerea articolului');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center p-4">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600 font-medium">Se încarcă comenzile...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Comenzile Mele</h1>
          <p className="text-gray-600">Urmărește-ți comenzile și statusul lor</p>
        </div>

        {orders.length === 0 ? (
          <div className="text-center bg-white rounded-2xl p-12 shadow-lg border border-gray-200">
            <div className="text-6xl mb-4">📭</div>
            <h3 className="text-2xl font-semibold text-gray-800 mb-2">Nu ai comenzi</h3>
            <p className="text-gray-600 mb-6">Plasează-ți prima comandă pentru a o vedea aici!</p>
            <button
              onClick={() => navigate('/menu')}
              className="bg-gradient-to-r from-blue-500 to-blue-600 text-white px-6 py-3 rounded-lg font-semibold hover:from-blue-600 hover:to-blue-700 transition duration-300"
            >
              Mergeti la Meniu
            </button>
          </div>
        ) : (
          <>
            {/* Filter Buttons */}
            <div className="mb-6 flex gap-2 flex-wrap">
              <button
                onClick={() => setFilter('all')}
                className={`px-4 py-2 rounded-full font-medium transition duration-300 ${
                  filter === 'all'
                    ? 'bg-blue-600 text-white shadow-lg'
                    : 'bg-white text-gray-700 border border-gray-300 hover:border-blue-600'
                }`}
              >
                Toate
              </button>
              <button
                onClick={() => setFilter('plasat')}
                className={`px-4 py-2 rounded-full font-medium transition duration-300 ${
                  filter === 'plasat'
                    ? 'bg-blue-600 text-white shadow-lg'
                    : 'bg-white text-gray-700 border border-gray-300 hover:border-blue-600'
                }`}
              >
                📋 Plasate
              </button>
              <button
                onClick={() => setFilter('plătit')}
                className={`px-4 py-2 rounded-full font-medium transition duration-300 ${
                  filter === 'plătit'
                    ? 'bg-blue-600 text-white shadow-lg'
                    : 'bg-white text-gray-700 border border-gray-300 hover:border-blue-600'
                }`}
              >
                💳 Plătite
              </button>
              <button
                onClick={() => setFilter('se prepară')}
                className={`px-4 py-2 rounded-full font-medium transition duration-300 ${
                  filter === 'se prepară'
                    ? 'bg-blue-600 text-white shadow-lg'
                    : 'bg-white text-gray-700 border border-gray-300 hover:border-blue-600'
                }`}
              >
                👨‍🍳 Se Prepară
              </button>
              <button
                onClick={() => setFilter('gata')}
                className={`px-4 py-2 rounded-full font-medium transition duration-300 ${
                  filter === 'gata'
                    ? 'bg-blue-600 text-white shadow-lg'
                    : 'bg-white text-gray-700 border border-gray-300 hover:border-blue-600'
                }`}
              >
                ✅ Gata
              </button>
            </div>

            {/* Orders List */}
            <div className="space-y-4">
              {filteredOrders.map(order => (
                <div
                  key={order.id}
                  className="bg-white rounded-2xl shadow-md hover:shadow-xl transition duration-300 border border-gray-200 overflow-hidden"
                >
                  {/* Order Header */}
                  <button
                    onClick={() => setExpandedOrder(expandedOrder === order.id ? null : order.id)}
                    className="w-full p-6 hover:bg-gray-50 transition duration-200"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-4 flex-1">
                        <div className="text-4xl">
                          {getStatusIcon(order.status)}
                        </div>
                        <div className="text-left">
                          <div className="flex items-center gap-3">
                            <h3 className="text-lg font-semibold text-gray-800">
                              Comandă #{order.id}
                            </h3>
                            <span className={`px-3 py-1 rounded-full text-sm font-semibold border ${getStatusColor(order.status)}`}>
                              {getStatusLabel(order.status)}
                            </span>
                          </div>
                          <p className="text-sm text-gray-500 mt-1">
                            📅 {new Date(order.orderDate).toLocaleDateString('ro-RO', {
                              year: 'numeric',
                              month: 'long',
                              day: 'numeric',
                              hour: '2-digit',
                              minute: '2-digit'
                            })}
                          </p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="text-2xl font-bold text-blue-600">
                          {order.totalAmount.toFixed(2)} lei
                        </p>
                        <p className="text-sm text-gray-500 mt-1">
                          {order.orderItems?.length || 0} articole
                        </p>
                      </div>
                    </div>
                  </button>

                  {/* Order Details (Expandable) */}
                  {expandedOrder === order.id && (
                    <div className="border-t border-gray-200 p-6 bg-gray-50">
                      {/* Items */}
                      <div className="mb-6">
                        <h4 className="font-semibold text-gray-800 mb-4 text-lg">Articole comandate:</h4>
                        <div className="space-y-3">
                          {order.orderItems?.map(item => (
                            <div
                              key={item.id}
                              className="flex justify-between items-center bg-white p-4 rounded-lg border border-gray-200"
                            >
                              <div className="flex-1">
                                <div className="flex items-center gap-3">
                                  <span className="inline-flex items-center justify-center w-8 h-8 bg-blue-100 text-blue-600 rounded-full text-sm font-semibold">
                                    {item.quantity}x
                                  </span>
                                  <span className="font-medium text-gray-800">
                                    {item.menuItem?.name || 'Articol Necunoscut'}
                                  </span>
                                </div>
                                {item.menuItem?.description && (
                                  <p className="text-sm text-gray-500 mt-1 ml-11">
                                    {item.menuItem.description}
                                  </p>
                                )}
                              </div>
                              <div className="text-right">
                                <p className="font-semibold text-gray-800">
                                  {(item.price * item.quantity).toFixed(2)} lei
                                </p>
                                <p className="text-sm text-gray-500">
                                  {item.price.toFixed(2)} lei/buc
                                </p>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>

                      {/* Notes if any */}
                      {order.notes && (
                        <div className="mb-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
                          <p className="text-sm text-gray-600">
                            <span className="font-semibold text-gray-800">Note speciale: </span>
                            {order.notes}
                          </p>
                        </div>
                      )}

                      {/* Summary */}
                      <div className="bg-white p-4 rounded-lg border border-gray-200">
                        <div className="flex justify-between items-center">
                          <span className="text-lg font-semibold text-gray-800">Total:</span>
                          <span className="text-2xl font-bold text-blue-600">
                            {order.totalAmount.toFixed(2)} lei
                          </span>
                        </div>
                      </div>

                      {/* Action Buttons */}
                      {order.status === OrderStatus.Pending && (
                        <div className="mt-4 flex gap-3">
                          <button
                            onClick={() => navigate('/cart')}
                            className="flex-1 bg-blue-600 text-white py-2 rounded-lg font-semibold hover:bg-blue-700 transition duration-300"
                          >
                            Modifică Comanda
                          </button>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Stats */}
            <div className="mt-8 grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="bg-white p-6 rounded-2xl shadow-md border border-gray-200">
                <p className="text-gray-600 text-sm font-medium mb-2">Total Comenzi</p>
                <p className="text-3xl font-bold text-gray-900">{orders.length}</p>
              </div>
              <div className="bg-white p-6 rounded-2xl shadow-md border border-gray-200">
                <p className="text-gray-600 text-sm font-medium mb-2">Finalizate</p>
                <p className="text-3xl font-bold text-emerald-600">
                  {orders.filter(o => o.status === OrderStatus.Completed).length}
                </p>
              </div>
              <div className="bg-white p-6 rounded-2xl shadow-md border border-gray-200">
                <p className="text-gray-600 text-sm font-medium mb-2">În Progres</p>
                <p className="text-3xl font-bold text-purple-600">
                  {orders.filter(o => o.status === OrderStatus.Preparing).length}
                </p>
              </div>
              <div className="bg-white p-6 rounded-2xl shadow-md border border-gray-200">
                <p className="text-gray-600 text-sm font-medium mb-2">Valoare Totală</p>
                <p className="text-3xl font-bold text-blue-600">
                  {orders.reduce((sum, o) => sum + o.totalAmount, 0).toFixed(2)} lei
                </p>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default Orders;
