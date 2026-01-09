# Frontend Integration Guide: Loyalty Points Discount

## Overview
This guide shows how to integrate the loyalty points discount feature into the payment flow.

## Backend API (Already Implemented)

### Get Loyalty Balance
```typescript
GET /api/loyalty/account
Authorization: Bearer <token>

Response:
{
  "id": 1,
  "userId": "guid",
  "pointsBalance": 250,
  "createdAt": "2026-01-08T...",
  "updatedAt": "2026-01-08T..."
}
```

### Use Points During Payment
```typescript
POST /api/payments/create-payment-intent/stripe
Authorization: Bearer <token>
Content-Type: application/json

Body:
{
  "orderId": 123,
  "loyaltyPointsToUse": 100  // Optional: points to redeem
}

Response: "pi_xxx_secret_yyy" (clientSecret string)
```

## Frontend Implementation

### Step 1: Add Loyalty Service
Create `src/services/loyaltyService.ts`:

```typescript
import { apiClient } from '../utils/apiClient';

export interface LoyaltyAccount {
  id: number;
  userId: string;
  pointsBalance: number;
  createdAt: string;
  updatedAt: string;
}

export interface LoyaltyTransaction {
  id: number;
  loyaltyAccountId: number;
  points: number;
  transactionType: string;
  description: string;
  createdAt: string;
}

export const loyaltyService = {
  async getAccount(): Promise<LoyaltyAccount | null> {
    try {
      const response = await apiClient('/api/loyalty/account');
      if (response.ok) {
        return await response.json();
      }
      return null;
    } catch (error) {
      console.error('Error fetching loyalty account:', error);
      return null;
    }
  },

  async getTransactions(): Promise<LoyaltyTransaction[]> {
    try {
      const response = await apiClient('/api/loyalty/transactions');
      if (response.ok) {
        return await response.json();
      }
      return [];
    } catch (error) {
      console.error('Error fetching transactions:', error);
      return [];
    }
  },

  calculateDiscount(points: number): number {
    // $0.01 per point
    return points * 0.01;
  }
};
```

### Step 2: Update Payment Page
Modify `src/pages/Payment.tsx`:

```typescript
import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { loadStripe } from '@stripe/stripe-js';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { apiClient } from '../utils/apiClient';
import { useToast } from '../context/ToastContext';
import { loyaltyService, LoyaltyAccount } from '../services/loyaltyService';

const stripePromise = loadStripe('pk_test_...');

const PaymentFormInner: React.FC<{ 
  loyaltyAccount: LoyaltyAccount | null;
  orderAmount: number;
}> = ({ loyaltyAccount, orderAmount }) => {
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [pointsToUse, setPointsToUse] = useState(0);

  const maxPointsToUse = loyaltyAccount 
    ? Math.min(loyaltyAccount.pointsBalance, Math.floor(orderAmount / 0.01))
    : 0;

  const discount = loyaltyService.calculateDiscount(pointsToUse);
  const finalAmount = Math.max(0, orderAmount - discount);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsProcessing(true);

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
      {/* Loyalty Points Section */}
      {loyaltyAccount && loyaltyAccount.pointsBalance > 0 && (
        <div className="bg-green-50 p-4 rounded-md border border-green-200">
          <h3 className="font-semibold text-green-800 mb-3">
            Use Loyalty Points
          </h3>
          <div className="space-y-3">
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Available Points:</span>
              <span className="font-semibold text-green-700">
                {loyaltyAccount.pointsBalance} points
              </span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Points Value:</span>
              <span className="font-semibold">
                ${loyaltyService.calculateDiscount(loyaltyAccount.pointsBalance).toFixed(2)}
              </span>
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Points to use (max: {maxPointsToUse})
              </label>
              <input
                type="number"
                min="0"
                max={maxPointsToUse}
                value={pointsToUse}
                onChange={(e) => {
                  const value = parseInt(e.target.value) || 0;
                  setPointsToUse(Math.min(Math.max(0, value), maxPointsToUse));
                }}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-green-500 focus:border-green-500"
                placeholder="0"
              />
            </div>

            {pointsToUse > 0 && (
              <div className="bg-white p-3 rounded border border-green-300">
                <div className="flex justify-between text-sm mb-1">
                  <span>Discount Applied:</span>
                  <span className="font-semibold text-green-600">
                    -${discount.toFixed(2)}
                  </span>
                </div>
                <div className="flex justify-between font-semibold">
                  <span>Final Amount:</span>
                  <span className="text-lg">${finalAmount.toFixed(2)}</span>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Payment Element */}
      <div className="bg-gray-50 p-4 rounded-md border border-gray-200">
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
          {isProcessing ? 'Processing...' : `Pay $${finalAmount.toFixed(2)}`}
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
  const [loyaltyAccount, setLoyaltyAccount] = useState<LoyaltyAccount | null>(null);
  const [orderAmount, setOrderAmount] = useState(0);
  const [pointsToUse, setPointsToUse] = useState(0);

  useEffect(() => {
    if (!orderId) {
      showToast('No order ID found', 'error');
      navigate('/orders');
      return;
    }

    const fetchData = async () => {
      try {
        // Fetch loyalty account
        const account = await loyaltyService.getAccount();
        setLoyaltyAccount(account);

        // Fetch order to get amount
        const orderResponse = await apiClient(`/api/orders/${orderId}`);
        if (orderResponse.ok) {
          const order = await orderResponse.json();
          setOrderAmount(order.totalAmount || 0);
        }
      } catch (err) {
        console.error('Error fetching data:', err);
      }
    };

    fetchData();
  }, [orderId, navigate, showToast]);

  useEffect(() => {
    if (!orderId) return;

    const fetchClientSecret = async () => {
      try {
        const payload: any = { orderId };
        
        // Include loyalty points if user wants to use them
        if (pointsToUse > 0) {
          payload.loyaltyPointsToUse = pointsToUse;
        }

        const response = await apiClient('/api/payments/create-payment-intent/stripe', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload),
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.message || 'Failed to initiate payment session');
        }

        const data = await response.json();
        setClientSecret(data);
      } catch (err: any) {
        console.error(err);
        setError(err.message || 'Could not load payment information. Please try again later.');
        showToast(err.message || 'Payment setup failed', 'error');
      }
    };

    fetchClientSecret();
  }, [orderId, pointsToUse, showToast]);

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
            <PaymentFormInner 
              loyaltyAccount={loyaltyAccount}
              orderAmount={orderAmount}
            />
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
```

### Step 3: Add Loyalty Points Display Component
Create `src/components/LoyaltyBadge.tsx`:

```typescript
import React, { useEffect, useState } from 'react';
import { loyaltyService, LoyaltyAccount } from '../services/loyaltyService';

export const LoyaltyBadge: React.FC = () => {
  const [account, setAccount] = useState<LoyaltyAccount | null>(null);

  useEffect(() => {
    const fetchAccount = async () => {
      const data = await loyaltyService.getAccount();
      setAccount(data);
    };
    fetchAccount();
  }, []);

  if (!account || account.pointsBalance === 0) {
    return null;
  }

  return (
    <div className="bg-green-100 text-green-800 px-3 py-1 rounded-full text-sm font-medium">
      🎁 {account.pointsBalance} points (${loyaltyService.calculateDiscount(account.pointsBalance).toFixed(2)})
    </div>
  );
};
```

## Key Points

1. **Loyalty points are optional** - users can choose to use them or not
2. **Points are deducted immediately** when creating payment intent
3. **Maximum points** is limited by either available balance or order amount
4. **Discount rate**: 100 points = $1 discount (configurable on backend)
5. **Points are earned automatically** when orders are completed

## Testing

1. Complete an order to earn points
2. View your points at `/api/loyalty/account`
3. During next payment, enter points to use
4. Verify discount is applied correctly
5. Check transaction history at `/api/loyalty/transactions`
