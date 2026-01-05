#!/bin/bash

# Test script to verify SonarQube setup and coverage generation
# This script tests the setup locally without connecting to SonarQube

set -euo pipefail

echo "=========================================="
echo "CampusEats - SonarQube Setup Test"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 1. Check if dotnet is installed
echo "1. Checking .NET installation..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}✗ .NET is not installed${NC}"
    exit 1
fi
dotnet --version
echo -e "${GREEN}✓ .NET is installed${NC}"
echo ""

# 2. Check if Node.js is installed
echo "2. Checking Node.js installation..."
if ! command -v node &> /dev/null; then
    echo -e "${RED}✗ Node.js is not installed${NC}"
    exit 1
fi
node --version
echo -e "${GREEN}✓ Node.js is installed${NC}"
echo ""

# 3. Restore dotnet tools
echo "3. Restoring .NET tools (including SonarScanner)..."
dotnet tool restore
echo -e "${GREEN}✓ Tools restored${NC}"
echo ""

# 4. Verify SonarScanner installation
echo "4. Verifying SonarScanner installation..."
if dotnet dotnet-sonarscanner 2>&1 | grep -q "SonarScanner for .NET"; then
    echo -e "${GREEN}✓ SonarScanner is installed${NC}"
else
    echo -e "${RED}✗ SonarScanner installation failed${NC}"
    exit 1
fi
echo ""

# 5. Restore .NET dependencies
echo "5. Restoring .NET dependencies..."
dotnet restore
echo -e "${GREEN}✓ Dependencies restored${NC}"
echo ""

# 6. Build the solution
echo "6. Building .NET solution..."
dotnet build --configuration Release --no-restore
echo -e "${GREEN}✓ Build successful${NC}"
echo ""

# 7. Run tests with coverage
echo "7. Running .NET tests with coverage generation..."
dotnet test CampusEats.Test/CampusEats.Test.csproj \
    --configuration Release \
    --no-restore \
    --no-build \
    --verbosity normal \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=opencover \
    /p:CoverletOutput=./coverage.opencover.xml

echo -e "${GREEN}✓ Tests completed with coverage${NC}"
echo ""

# 8. Verify coverage file exists
echo "8. Verifying coverage report generation..."
if [ -f "CampusEats.Test/coverage.opencover.xml" ]; then
    COVERAGE_FILE_SIZE=$(wc -c < "CampusEats.Test/coverage.opencover.xml")
    echo -e "${GREEN}✓ Coverage report generated (${COVERAGE_FILE_SIZE} bytes)${NC}"
    echo "  Location: CampusEats.Test/coverage.opencover.xml"
else
    echo -e "${RED}✗ Coverage report not found${NC}"
    exit 1
fi
echo ""

# 9. Test React client (optional)
echo "9. Testing React client coverage generation..."
cd campuseats.client
if [ ! -d "node_modules" ]; then
    echo "  Installing npm dependencies..."
    npm ci
fi

echo "  Running React tests with coverage..."
npm test -- --coverage --watchAll=false --passWithNoTests --coverageReporters=lcov 2>&1 | grep -E "(PASS|FAIL|Test Suites)" || true

if [ -f "coverage/lcov.info" ]; then
    LCOV_FILE_SIZE=$(wc -c < "coverage/lcov.info")
    echo -e "${GREEN}✓ React coverage report generated (${LCOV_FILE_SIZE} bytes)${NC}"
    echo "  Location: campuseats.client/coverage/lcov.info"
else
    echo -e "${YELLOW}⚠ React coverage report not generated (no tests)${NC}"
fi
cd ..
echo ""

# 10. Check sonar-project.properties
echo "10. Verifying SonarQube configuration..."
if [ -f "sonar-project.properties" ]; then
    echo -e "${GREEN}✓ sonar-project.properties found${NC}"
    echo "  Configuration:"
    grep "^sonar.projectKey" sonar-project.properties || echo "  (no project key set)"
    grep "^sonar.cs.opencover.reportsPaths" sonar-project.properties || echo "  (no coverage path set)"
else
    echo -e "${RED}✗ sonar-project.properties not found${NC}"
    exit 1
fi
echo ""

# Summary
echo "=========================================="
echo -e "${GREEN}All checks passed!${NC}"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Configure GitHub Secrets (SONAR_TOKEN, SONAR_HOST_URL)"
echo "2. Set up project on SonarCloud or SonarQube"
echo "3. Push to main/develop or create a PR"
echo ""
echo "For detailed instructions, see:"
echo "  - SONARQUBE_QUICKSTART.md"
echo "  - SONARQUBE_SETUP.md"
echo ""

# Cleanup
echo "Cleaning up test artifacts..."
rm -f CampusEats.Test/coverage.opencover.xml
rm -rf campuseats.client/coverage
echo -e "${GREEN}✓ Cleanup complete${NC}"
