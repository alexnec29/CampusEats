import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { loadStripe } from '@stripe/stripe-js';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { apiClient } from '../utils/apiClient';

const stripePromise = loadStripe('pk_test_51ScmzvGeihajtF8vETlsa6FKEZkyQNnMVNEo35DxraZ8qZQs6vhovSNFOfqMmFX684XhuIRzxU5YBvnXcTGf5v7A00v1m3wMH6');

const PaymentFormInner: React.FC = () => {
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();

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
  
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!orderId) {
      alert('No order ID found');
      navigate('/orders');
      return;
    }

    const fetchClientSecret = async () => {
      try {
        const response = await apiClient('/api/payments/create-payment-intent/stripe', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ orderId: orderId }),
        });

        if (!response.ok) {
          throw new Error('Failed to initiate payment session');
        }

        const data = await response.json();
        setClientSecret(data);
        console.log(data);
      } catch (err) {
        console.error(err);
        setError('Could not load payment information. Please try again later.');
      }
    };

    fetchClientSecret();
  }, [orderId, navigate]);

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
        <p className="text-center text-gray-500 mb-8">Order ID: #{orderId}</p>
        
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