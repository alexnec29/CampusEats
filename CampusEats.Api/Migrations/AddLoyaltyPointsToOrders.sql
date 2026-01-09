-- Migration: Add loyalty points fields to Orders table
-- Date: 2026-01-09
-- Description: Adds LoyaltyPointsDiscount and RedeemedLoyaltyPoints columns to support loyalty points redemption in cart

-- Add LoyaltyPointsDiscount column if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Orders' AND column_name = 'LoyaltyPointsDiscount'
    ) THEN
        ALTER TABLE "Orders" 
        ADD COLUMN "LoyaltyPointsDiscount" DECIMAL(10,2) NOT NULL DEFAULT 0;
    END IF;
END $$;

-- Add RedeemedLoyaltyPoints column if it doesn't exist
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Orders' AND column_name = 'RedeemedLoyaltyPoints'
    ) THEN
        ALTER TABLE "Orders" 
        ADD COLUMN "RedeemedLoyaltyPoints" INTEGER NOT NULL DEFAULT 0;
    END IF;
END $$;

-- Verify the columns were added
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'Orders' 
  AND column_name IN ('LoyaltyPointsDiscount', 'RedeemedLoyaltyPoints')
ORDER BY column_name;
