# Database Migrations

This directory contains SQL migration scripts for database schema changes.

## How to Apply Migrations

### For Local Development (Docker)

If you're using Docker, the easiest way is to restart the database with a clean slate:

```bash
# Stop services and remove volumes
docker-compose down -v

# Start services (database will be recreated)
docker-compose up -d
```

### For Existing Database

If you have an existing database with data you want to keep, apply the migration manually:

1. Connect to your PostgreSQL database:
   ```bash
   docker exec -it campuseats-db-1 psql -U postgres -d campuseats_db
   ```

2. Run the migration script:
   ```bash
   docker exec -i campuseats-db-1 psql -U postgres -d campuseats_db < CampusEats.Api/Migrations/AddLoyaltyPointsToOrders.sql
   ```

   Or copy the SQL commands from `AddLoyaltyPointsToOrders.sql` and paste them into your PostgreSQL client.

3. Verify the columns were added:
   ```sql
   \d "Orders"
   ```

## Current Migrations

### AddLoyaltyPointsToOrders.sql
- **Date**: 2026-01-09
- **Description**: Adds `LoyaltyPointsDiscount` and `RedeemedLoyaltyPoints` columns to the Orders table
- **Required for**: Loyalty points redemption feature in cart page

## Note

This project currently uses `EnsureCreated()` for database initialization, which means:
- New databases are automatically created with the latest schema
- Existing databases are **not** automatically updated
- You must manually apply migrations to existing databases or recreate them
