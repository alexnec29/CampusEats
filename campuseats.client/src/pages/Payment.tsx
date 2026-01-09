import React, { useEffect, useState, useCallback } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { loadStripe } from '@stripe/stripe-js';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { apiClient } from '../utils/apiClient';
import { useToast } from '../context/ToastContext';

const stripePromise = loadStripe('pk_test_51ScmzvGeihajtF8vETlsa6FKEZkyQNnMVNEo35DxraZ8qZQs6vhovSNFOfqMmFX684XhuIRzxU5YBvnXcTGf5v7A00v1m3wMH6');

const PaymentFormInner: React.FC = () => {
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!stripe || !elements) {
      // Stripe.js has not loaded yet. Make sure to disable form submission until Stripe.js has loaded.
      return;
    }

    setIsProcessing(true);

    // Confirm the payment
    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: `${window.location.origin}/orders`,
      },
    });

    if (error) {
      setErrorMessage(error.message || "An unexpected error occurred.");
      showToast(error.message || "Payment failed", 'error');
      setIsProcessing(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="bg-gray-50 p-4 rounded-md border border-gray-200">
        {/* This component renders the secure Card Number, Expiry, CVC, and Zip inputs */}
        <PaymentElement />
      </div>

      {errorMessage && (
        <div className="text-red-600 text-sm bg-red-50 p-3 rounded border border-red-200">
          {errorMessage}
        </div>
      )}

      <div className="flex justify-between items-center mt-6">
        <button
          type="button"
          onClick={() => navigate('/orders')}
          className="text-gray-600 hover:text-gray-800 transition-colors"
        >
          Cancel
        </button>

        <button
          type="submit"
          disabled={!stripe || isProcessing}
          className={`px-6 py-2 rounded text-white font-medium transition-colors ${
            isProcessing || !stripe
              ? 'bg-gray-400 cursor-not-allowed'
              : 'bg-green-600 hover:bg-green-700'
          }`}
        >
          {isProcessing ? 'Processing...' : 'Pay Now'}
        </button>
      </div>
    </form>
  );
};

const PaymentPage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const orderId = location.state?.orderId;
  const { showToast } = useToast();
  
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loyaltyPoints, setLoyaltyPoints] = useState<number>(0);
  const [pointsToRedeem, setPointsToRedeem] = useState<number>(0);
  const [orderTotal, setOrderTotal] = useState<number>(0);
  const [loadingLoyalty, setLoadingLoyalty] = useState(true);

  useEffect(() => {
    if (!orderId) {
      showToast('No order ID found', 'error');
      navigate('/orders');
      return;
    }

    const fetchLoyaltyAndOrder = async () => {
      try {
        // Fetch loyalty points
        const loyaltyRes = await apiClient('/api/loyalty/account');
        if (loyaltyRes.ok) {
          const loyaltyData = await loyaltyRes.json();
          setLoyaltyPoints(loyaltyData.pointsBalance || 0);
        }
      } catch (err) {
        console.error('Error fetching loyalty:', err);
      } finally {
        setLoadingLoyalty(false);
      }

      try {
        // Fetch order details to get total
        const orderRes = await apiClient(`/api/orders/${orderId}`);
        if (orderRes.ok) {
          const orderData = await orderRes.json();
          setOrderTotal(orderData.totalAmount || 0);
        }
      } catch (err) {
        console.error('Error fetching order:', err);
      }
    };

    fetchLoyaltyAndOrder();
  }, [orderId, navigate, showToast]);

  const createPaymentIntent = useCallback(async () => {
    try {
      const response = await apiClient('/api/payments/create-payment-intent/stripe', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          orderId: orderId,
          loyaltyPointsToRedeem: pointsToRedeem > 0 ? pointsToRedeem : null
        }),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Failed to initiate payment session');
      }

      const data = await response.json();
      setClientSecret(data);
      console.log(data);
    } catch (err: any) {
      console.error(err);
      setError(err.message || 'Could not load payment information. Please try again later.');
    }
  }, [orderId, pointsToRedeem]);

  useEffect(() => {
    if (!orderId) {
      return;
    }
    
    if (!loadingLoyalty) {
      createPaymentIntent();
    }
  }, [orderId, loadingLoyalty, createPaymentIntent]);

  const handleRedeemChange = (value: number) => {
    const maxPoints = Math.min(loyaltyPoints, Math.floor(orderTotal * 100)); // Can't redeem more than order total
    const newValue = Math.max(0, Math.min(value, maxPoints));
    setPointsToRedeem(newValue);
  };

  const applyPoints = () => {
    if (pointsToRedeem > 0) {
      setClientSecret(null); // Reset to trigger new payment intent
      setTimeout(() => {
        createPaymentIntent();
      }, 100);
    }
  };

  const discount = pointsToRedeem / 100;
  const finalAmount = Math.max(0, orderTotal - discount);

  const options = {
    clientSecret: clientSecret || "",
    appearance: {
      theme: 'stripe' as const,
      variables: {
        colorPrimary: '#16a34a',
      },
    },
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
      <div className="bg-white p-8 rounded-lg shadow-md max-w-md w-full">
        <h2 className="text-3xl font-bold mb-2 text-center text-gray-800">Secure Payment</h2>
        <p className="text-center text-gray-500 mb-6">Order ID: #{orderId}</p>
        
        {/* Loyalty Points Section */}
        {!loadingLoyalty && loyaltyPoints > 0 && (
          <div className="mb-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
            <div className="flex justify-between items-center mb-2">
              <span className="font-semibold text-gray-700">Available Points:</span>
              <span className="text-blue-600 font-bold">{loyaltyPoints}</span>
            </div>
            <div className="mb-3">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Redeem Points (100 points = $1.00)
              </label>
              <input
                type="number"
                min="0"
                max={Math.min(loyaltyPoints, Math.floor(orderTotal * 100))}
                value={pointsToRedeem}
                onChange={(e) => handleRedeemChange(parseInt(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="0"
              />
            </div>
            {pointsToRedeem > 0 && (
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span>Original Total:</span>
                  <span className="font-medium">${orderTotal.toFixed(2)}</span>
                </div>
                <div className="flex justify-between text-green-600">
                  <span>Discount:</span>
                  <span className="font-medium">-${discount.toFixed(2)}</span>
                </div>
                <div className="flex justify-between border-t pt-2 font-bold text-lg">
                  <span>Final Total:</span>
                  <span className="text-blue-600">${finalAmount.toFixed(2)}</span>
                </div>
                <button
                  onClick={applyPoints}
                  className="w-full mt-2 bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 transition-colors"
                >
                  Apply Points
                </button>
              </div>
            )}
          </div>
        )}
        
        {error ? (
          <div className="text-red-600 text-center bg-red-50 p-4 rounded">
            {error}
          </div>
        ) : clientSecret ? (
          <Elements stripe={stripePromise} options={options}>
            <PaymentFormInner />
          </Elements>
        ) : (
          <div className="flex justify-center items-center py-10">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600"></div>
            <span className="ml-3 text-gray-600">Loading secure checkout...</span>
          </div>
        )}
      </div>
    </div>
  );
};

export default PaymentPage;