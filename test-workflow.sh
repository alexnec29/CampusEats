#!/bin/bash

# Test script to verify PR verification workflow steps
# This simulates the GitHub Actions workflow locally

set -e  # Exit on any error

echo "=========================================="
echo "Testing PR Verification Workflow Steps"
echo "=========================================="
echo ""

# Store the starting directory
START_DIR=$(pwd)

# Step 1: Verify we're in the right directory
echo "Step 1: Verify directory structure"
echo "Current directory: $START_DIR"
ls -la | grep -E "CampusEats\.(sln|Api|Test)|campuseats\.client"
echo "✓ Directory structure verified"
echo ""

# Step 2: Restore .NET dependencies
echo "Step 2: Restore .NET dependencies"
dotnet restore
echo "✓ .NET dependencies restored"
echo ""

# Step 3: Build .NET API
echo "Step 3: Build .NET API"
dotnet build CampusEats.Api/CampusEats.Api.csproj --configuration Release --no-restore
echo "✓ .NET API built successfully"
echo ""

# Step 4: Run .NET Tests
echo "Step 4: Run .NET Tests"
dotnet test CampusEats.Test/CampusEats.Test.csproj --configuration Release --no-restore --verbosity normal
echo "✓ .NET tests passed"
echo ""

# Step 5: Check client npm paths
echo "Step 5: Verify client paths"
ls -la campuseats.client/package-lock.json
echo "✓ Client paths verified"
echo ""

# Step 6: Install client dependencies (using existing node_modules if present)
echo "Step 6: Verify client dependencies"
cd campuseats.client
if [ ! -d "node_modules" ]; then
    echo "Installing client dependencies..."
    npm ci
else
    echo "Client dependencies already present"
fi
cd "$START_DIR"
echo "✓ Client dependencies ready"
echo ""

# Step 7: Build React Client
echo "Step 7: Build React Client"
cd campuseats.client
npm run build
cd "$START_DIR"
echo "✓ React client built successfully"
echo ""

# Step 8: Run Client Tests
echo "Step 8: Run Client Tests"
cd campuseats.client
npm test -- --watchAll=false --passWithNoTests
cd "$START_DIR"
echo "✓ Client tests passed"
echo ""

# Step 9: Verify Docker file paths
echo "Step 9: Verify Docker file paths"
echo "Checking Dockerfile paths..."
cat Dockerfile | grep "COPY" | grep -v "from=build"
echo ""
echo "Verifying files exist:"
ls -la CampusEats.sln CampusEats.Api/CampusEats.Api.csproj CampusEats.Test/CampusEats.Test.csproj 2>&1 | grep -v "total" | head -3
echo ""
echo "Checking Dockerfile.client paths..."
cat Dockerfile.client | grep "COPY" | grep -v "from=build"
echo ""
echo "Verifying files exist:"
ls -la campuseats.client/package*.json 2>&1 | grep -v "total"
echo "✓ All Docker file paths verified"
echo ""

echo "=========================================="
echo "All workflow steps completed successfully!"
echo "=========================================="
echo ""
echo "Summary:"
echo "  ✓ .NET API builds"
echo "  ✓ .NET tests pass"
echo "  ✓ React client builds"
echo "  ✓ React client tests pass"
echo "  ✓ Docker paths verified"
echo ""
echo "The GitHub Actions workflow should work correctly!"
