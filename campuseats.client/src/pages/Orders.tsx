import React from 'react';
import { useOrders } from '../hooks/useOrders';
import { OrderStatus } from '../types';

const Orders: React.FC = () => {
  const { orders, loading, removeItem, getStatusLabel, getStatusColor } = useOrders();

  if (loading) {
    return <div className="p-4">Loading orders...</div>;
  }

  if (orders.length === 0) {
    return (
      <div className="p-4">
        <h2 className="text-2xl font-bold mb-6">My Orders</h2>
        <p>No orders found.</p>
      </div>
    );
  }
  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold mb-6">My Orders</h2>
      <div className="space-y-4">
        {orders.map(order => (
          <div key={order.id} className="border rounded-lg p-4 shadow-sm">
            <div className="flex justify-between items-start mb-4">
              <div>
                <span className="text-sm text-gray-500">Order #{order.id}</span>
                <p className="text-sm text-gray-500">{new Date(order.orderDate).toLocaleString()}</p>
              </div>
              <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(order.status)}`}>
                {getStatusLabel(order.status)}
              </span>
            </div>

            <div className="border-t pt-4">
              <h4 className="font-semibold mb-2">Items:</h4>
              <ul className="space-y-2">
                {order.orderItems?.map(item => (
                  <li key={item.id} className="flex justify-between text-sm items-center">
                    <span>{item.quantity}x {item.menuItem?.name || 'Unknown Item'}</span>
                    <div className="flex items-center gap-4">
                      <span>${(item.price * item.quantity).toFixed(2)}</span>
                      {order.status === OrderStatus.Pending && (
                        <button
                          onClick={() => removeItem(order.id, item.id)}
                          className="text-red-600 hover:text-red-800 text-xs font-semibold"
                        >
                          Remove
                        </button>
                      )}
                    </div>
                  </li>
                ))}
              </ul>
              <div className="border-t mt-4 pt-2 flex justify-between font-bold">
                <span>Total</span>
                <span>${order.totalAmount.toFixed(2)}</span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Orders;
