# Loyalty System Documentation

## Overview
The CampusEats loyalty system rewards buyers with points for completed orders and allows them to redeem those points for discounts on future purchases.

## Features

### 1. Earning Points
- **Automatic Point Earning**: When an order status changes to `Completed`, the system automatically awards loyalty points to the buyer
- **Configurable Earn Rate**: Points are earned based on the order total amount
  - Default: 1 point per $1 spent
  - Configured in `appsettings.json` under `Loyalty:PointsPerDollar`

### 2. Redeeming Points
- **Discount on Orders**: Buyers can use loyalty points to get discounts when making payments
- **Configurable Redemption Rate**: Points can be converted to dollar discounts
  - Default: $0.01 per point (100 points = $1 discount)
  - Configured in `appsettings.json` under `Loyalty:DollarsPerPoint`
- **Payment Integration**: Loyalty points are applied during the payment intent creation

### 3. Account Management
- **Automatic Account Creation**: Loyalty accounts are automatically created when a buyer earns points for the first time
- **Balance Tracking**: Real-time tracking of points balance
- **Transaction History**: All point earnings and redemptions are recorded

## API Endpoints

### Get Loyalty Account
```
GET /api/loyalty/account
Authorization: Required (Buyer role)
```
Returns the authenticated buyer's loyalty account with current points balance.

### Get Transaction History
```
GET /api/loyalty/transactions
Authorization: Required (Buyer role)
```
Returns all loyalty transactions for the authenticated buyer.

### Redeem Points
```
POST /api/loyalty/redeem
Authorization: Required (Buyer role)
Body: {
  "points": 100,
  "description": "Redeem for discount"
}
```
Manually redeems points (creates a transaction). Note: Points are automatically redeemed when used during payment.

### Adjust Points (Admin Only)
```
POST /api/loyalty/adjust
Authorization: Required (Admin role)
Body: {
  "userId": "guid",
  "points": 50,
  "reason": "Bonus points"
}
```
Allows admins to manually adjust a user's loyalty points balance.

## Usage Flow

### Earning Points Flow
1. Customer places an order
2. Order is prepared and marked as Ready
3. Order status is updated to Completed
4. System automatically:
   - Calculates points based on order total
   - Creates or updates loyalty account
   - Adds points to balance
   - Records transaction

### Redeeming Points Flow
1. Customer views their loyalty balance
2. Customer creates an order
3. During payment, customer specifies loyalty points to use
4. System:
   - Validates sufficient points
   - Calculates discount amount
   - Deducts points from balance
   - Applies discount to payment
   - Records redemption transaction

## Configuration

In `appsettings.json`:

```json
{
  "Loyalty": {
    "PointsPerDollar": 1,
    "DollarsPerPoint": 0.01
  }
}
```

### Configuration Options

| Setting | Description | Default | Example |
|---------|-------------|---------|---------|
| `PointsPerDollar` | How many points earned per dollar spent | 1 | If set to 2, a $50 order earns 100 points |
| `DollarsPerPoint` | Dollar value of each point when redeemed | 0.01 | 100 points = $1 discount |

## Database Schema

### LoyaltyAccount
- `Id`: Primary key
- `UserId`: Foreign key to User
- `PointsBalance`: Current points balance
- `CreatedAt`: Account creation timestamp
- `UpdatedAt`: Last modification timestamp

### LoyaltyTransaction
- `Id`: Primary key
- `LoyaltyAccountId`: Foreign key to LoyaltyAccount
- `Points`: Points amount (positive for earn, negative for redeem)
- `TransactionType`: "Earn", "Redeem", or "AdminAdjustment"
- `Description`: Transaction description
- `CreatedAt`: Transaction timestamp

## Implementation Details

### Key Components

1. **EarnPointsHandler** (`Features/Loyalty/EarnPoints/`)
   - Handles automatic point earning
   - Creates loyalty account if needed
   - Records earn transactions

2. **UpdateOrderStatusHandler** (`Features/Order/UpdateOrderStatus/`)
   - Triggers point earning when order completed
   - Uses MediatR to send EarnPointsRequest

3. **CreatePaymentIntentHandler** (`Features/Payment/Stripe/`)
   - Applies loyalty discount to payment
   - Validates and deducts points
   - Records redemption transactions

## Testing

The loyalty system includes comprehensive unit tests:

- `EarnPointsHandlerTests`: Tests point earning logic
- Integration with order status updates
- Payment integration tests
- All 102 tests passing ✓

## Example Scenarios

### Scenario 1: First Order
- Customer makes a $50 order
- Order is completed
- Customer earns 50 points
- Loyalty account is automatically created
- Balance: 50 points

### Scenario 2: Using Points
- Customer has 200 points
- Makes a new $30 order
- Uses 100 points during payment
- Receives $1 discount
- Final payment: $29
- New balance: 100 points
- Order completion earns 30 more points
- Final balance: 130 points

### Scenario 3: Admin Adjustment
- Admin awards 500 bonus points to customer
- Customer's balance increases by 500 points
- Transaction recorded with type "AdminAdjustment"

## Future Enhancements

Potential improvements for the loyalty system:
- Point expiration dates
- Tiered loyalty levels (Bronze, Silver, Gold)
- Special promotions (double points days)
- Referral bonuses
- Birthday rewards
- Minimum redemption thresholds
- Maximum discount limits per order
