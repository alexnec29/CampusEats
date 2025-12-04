# Makefile at the root of CampusEats

# Variables
API_PATH=CampusEats.Api
CLIENT_PATH=CampusEats.Client

# .NET SDK version
DOTNET_VERSION=9.0

# Node.js version
NODE_VERSION=20

# Targets
all: api client

# Build .NET API
api:
	dotnet restore $(API_PATH)/$(API_PATH).csproj
	dotnet build $(API_PATH)/$(API_PATH).csproj --configuration Release --no-restore
	dotnet test $(API_PATH)/$(API_PATH).csproj --no-build --verbosity normal

# Build React client
client:
	cd $(CLIENT_PATH) && npm install
	cd $(CLIENT_PATH) && npm run build
	cd $(CLIENT_PATH) && npm test -- --watchAll=false

.PHONY: all api client
