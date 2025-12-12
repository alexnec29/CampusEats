# Dockerfile for CampusEats .NET API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY CampusEats.sln ./
COPY CampusEats.Api/CampusEats.Api.csproj CampusEats.Api/
COPY CampusEats.Test/CampusEats.Test.csproj CampusEats.Test/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY CampusEats.Api/ CampusEats.Api/
COPY CampusEats.Test/ CampusEats.Test/

# Build the API
WORKDIR /src/CampusEats.Api
RUN dotnet build -c Release -o /app/build

# Run tests
WORKDIR /src/CampusEats.Test
RUN dotnet test --no-restore --verbosity normal

# Publish the API
WORKDIR /src/CampusEats.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5078
EXPOSE 5078

ENTRYPOINT ["dotnet", "CampusEats.Api.dll"]
