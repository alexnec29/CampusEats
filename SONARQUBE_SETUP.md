# SonarQube Setup Guide

This document explains how to set up and use SonarQube for code quality and coverage analysis in the CampusEats project.

## Overview

The project is configured to use SonarQube for:
- Static code analysis
- Code quality metrics
- Security vulnerability detection
- Code coverage tracking (both .NET and React)

## Prerequisites

1. **SonarQube Server**: You need access to a SonarQube server instance
   - Use [SonarCloud](https://sonarcloud.io/) (free for open-source projects)
   - Or self-host SonarQube server

2. **GitHub Secrets**: Configure the following secrets in your GitHub repository:
   - `SONAR_TOKEN`: Your SonarQube authentication token
   - `SONAR_HOST_URL`: Your SonarQube server URL (e.g., `https://sonarcloud.io`)

## Configuration Files

### 1. `sonar-project.properties`
Main configuration file containing:
- Project key and organization
- Source and test paths
- Code coverage report paths
- File exclusions

### 2. `.github/workflows/sonarqube.yml`
GitHub Actions workflow that:
- Runs on push to main/develop branches
- Runs on pull requests
- Builds the project
- Executes tests with coverage
- Sends results to SonarQube

### 3. `.config/dotnet-tools.json`
Includes `dotnet-sonarscanner` tool for .NET analysis

### 4. `CampusEats.Test/CampusEats.Test.csproj`
Updated to include `coverlet.msbuild` for coverage report generation

## Setup Instructions

### For SonarCloud (Recommended for Open Source)

1. **Create SonarCloud Account**
   - Go to [SonarCloud](https://sonarcloud.io/)
   - Sign in with your GitHub account

2. **Create New Project**
   - Click "+" → "Analyze new project"
   - Select your repository: `alexnec29/CampusEats`
   - Choose "With GitHub Actions"

3. **Get Authentication Token**
   - Go to Account → Security → Generate Token
   - Copy the token

4. **Configure GitHub Secrets**
   ```bash
   # In your GitHub repository: Settings → Secrets and variables → Actions
   SONAR_TOKEN: <your-token-from-step-3>
   SONAR_HOST_URL: https://sonarcloud.io
   ```

5. **Update Project Key** (if needed)
   - Update `sonar.projectKey` and `sonar.organization` in `sonar-project.properties`
   - Update the same values in `.github/workflows/sonarqube.yml`

### For Self-Hosted SonarQube

1. **Install SonarQube Server**
   - Follow [official installation guide](https://docs.sonarqube.org/latest/setup/install-server/)
   - Or use Docker: `docker run -d --name sonarqube -p 9000:9000 sonarqube:latest`

2. **Create Project**
   - Access SonarQube UI (default: http://localhost:9000)
   - Create new project and generate token

3. **Configure GitHub Secrets**
   ```bash
   SONAR_TOKEN: <your-project-token>
   SONAR_HOST_URL: <your-sonarqube-url>
   ```

4. **Update Project Key**
   - Update `sonar.projectKey` in `sonar-project.properties`
   - Remove `sonar.organization` if not using SonarCloud
   - Update `.github/workflows/sonarqube.yml` accordingly

## Running Analysis

### Automated (GitHub Actions)
Analysis runs automatically:
- On every push to `main` or `develop` branches
- On every pull request

### Manual (Local Development)

#### Prerequisites
```bash
# Install Java (required for SonarScanner)
sudo apt-get install openjdk-17-jdk  # Linux
# or
brew install openjdk@17  # macOS

# Install dotnet-sonarscanner
dotnet tool restore
```

#### Run Analysis
```bash
# 1. Begin analysis
dotnet sonarscanner begin \
  /k:"alexnec29_CampusEats" \
  /o:"alexnec29" \
  /d:sonar.host.url="<SONAR_HOST_URL>" \
  /d:sonar.token="<SONAR_TOKEN>" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

# 2. Build the solution
dotnet build --configuration Release

# 3. Run tests with coverage
dotnet test CampusEats.Test/CampusEats.Test.csproj \
  --configuration Release \
  --no-build \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage.opencover.xml

# 4. Run React tests with coverage (optional)
cd campuseats.client
npm test -- --coverage --watchAll=false --coverageReporters=lcov
cd ..

# 5. End analysis and upload
dotnet sonarscanner end /d:sonar.token="<SONAR_TOKEN>"
```

## Code Coverage

### .NET Backend
- Uses **Coverlet** for coverage collection
- Output format: **OpenCover XML**
- Report path: `CampusEats.Test/coverage.opencover.xml`

### React Frontend
- Uses **Jest** built-in coverage
- Output format: **LCOV**
- Report path: `campuseats.client/coverage/lcov.info`

## Viewing Results

1. **In SonarQube/SonarCloud Dashboard**
   - Navigate to your project
   - View metrics: Coverage, Code Smells, Bugs, Vulnerabilities
   - Check coverage per file/package

2. **In GitHub Pull Requests**
   - SonarCloud can add comments to PRs
   - Quality Gate status check
   - Configure in SonarCloud → Administration → Pull Requests

## Quality Gates

Default quality gate requires:
- No new bugs
- No new vulnerabilities
- Coverage on new code > 80%
- Code smell rating ≤ A

Configure custom gates in SonarQube/SonarCloud UI.

## Troubleshooting

### Coverage reports not showing
- Ensure tests run successfully
- Check coverage file paths in `sonar-project.properties`
- Verify coverage files are generated: `find . -name "coverage.opencover.xml"`

### Analysis fails
- Check `SONAR_TOKEN` and `SONAR_HOST_URL` secrets are set correctly
- Verify project key matches in all config files
- Ensure Java 17+ is installed

### Permission denied
- Generate a new token with appropriate permissions
- Check organization/project visibility settings

## Additional Resources

- [SonarQube Documentation](https://docs.sonarqube.org/latest/)
- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [Jest Coverage](https://jestjs.io/docs/configuration#collectcoverage-boolean)

## Support

For issues specific to this setup, please open an issue in the repository.
