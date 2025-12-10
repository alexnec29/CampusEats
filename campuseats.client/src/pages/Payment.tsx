import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { apiClient } from '../utils/apiClient';
import { OrderStatus } from '../types';

const Payment: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const orderId = location.state?.orderId;

  const handlePayment = async () => {
    if (!orderId) {
        alert('No order ID found');
        return;
    }

    try {
        const response = await apiClient(`/api/orders/${orderId}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: OrderStatus.Paid })
        });

        if (response.ok) {
            alert('Payment successful! Order status updated to Paid.');
            navigate('/orders');
        } else {
            alert('Failed to update order status');
        }
    } catch (error) {
        console.error('Error updating order status:', error);
        alert('Error updating order status');
    }
  };

  return (
    <div className="p-8 text-center">
      <h2 className="text-3xl font-bold mb-4">Payment Page</h2>
      <p className="text-xl mb-8">This is a placeholder for the online payment integration.</p>
      
      {orderId && (
          <button
            onClick={handlePayment}
            className="bg-green-600 text-white px-6 py-2 rounded hover:bg-green-700 mr-4"
          >
            Pay Now (Simulate)
          </button>
      )}

      <button
        onClick={() => navigate('/orders')}
        className="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700"
      >
        Go to My Orders
      </button>
    </div>
  );
};

export default Payment;
