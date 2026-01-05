# SonarQube Quick Start

Quick guide to get SonarQube scanning up and running for the CampusEats project.

## Prerequisites

- GitHub repository secrets configured:
  - `SONAR_TOKEN`: Your SonarQube/SonarCloud authentication token
  - `SONAR_HOST_URL`: Your SonarQube server URL (e.g., `https://sonarcloud.io`)

## Using SonarCloud (Recommended for Open Source)

1. Go to [SonarCloud](https://sonarcloud.io/) and sign in with GitHub
2. Click "+" → "Analyze new project" → Select `alexnec29/CampusEats`
3. Choose "With GitHub Actions"
4. Generate a token: Account → Security → Generate Token
5. Add the token to GitHub Secrets:
   - Go to: Repository Settings → Secrets and variables → Actions
   - Add `SONAR_TOKEN` with your token
   - Add `SONAR_HOST_URL` with value: `https://sonarcloud.io`
6. Push to `main`/`develop` or create a PR - analysis runs automatically!

## Viewing Results

- SonarCloud Dashboard: https://sonarcloud.io/organizations/alexnec29/projects
- Check "Checks" tab in GitHub PRs for quality gate status

## Local Analysis (Optional)

```bash
# Install tools
dotnet tool restore

# Run with environment variables
export SONAR_TOKEN="your-token"
export SONAR_HOST_URL="https://sonarcloud.io"

# Begin analysis
dotnet sonarscanner begin \
  /k:"alexnec29_CampusEats" \
  /o:"alexnec29" \
  /d:sonar.host.url="${SONAR_HOST_URL}" \
  /d:sonar.token="${SONAR_TOKEN}" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

# Build and test
dotnet build --configuration Release
dotnet test CampusEats.Test/CampusEats.Test.csproj \
  --configuration Release \
  --no-build \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage.opencover.xml

# End analysis
dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
```

## Documentation

For detailed setup instructions, troubleshooting, and advanced configuration, see [SONARQUBE_SETUP.md](./SONARQUBE_SETUP.md).

## Coverage Reports

- **Backend (.NET)**: OpenCover XML format
- **Frontend (React)**: LCOV format
- Reports are automatically generated and uploaded during CI/CD

## What Gets Analyzed

✅ C# code in `CampusEats.Api`  
✅ JavaScript/TypeScript in `campuseats.client/src`  
✅ Unit tests in `CampusEats.Test`  
✅ Code coverage metrics  
✅ Security vulnerabilities  
✅ Code smells and bugs  

❌ Excluded: node_modules, bin, obj, migrations, wwwroot
