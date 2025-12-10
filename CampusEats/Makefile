# Makefile at the root of the repo

API_PATH=CampusEats.Api
CLIENT_PATH=CampusEats.Client

all: api client

api:
	dotnet restore $(API_PATH)/$(API_PATH).csproj
	dotnet build $(API_PATH)/$(API_PATH).csproj --configuration Release --no-restore
	dotnet test $(API_PATH)/$(API_PATH).csproj --no-build --verbosity normal

client:
	npm install --prefix $(CLIENT_PATH)
	npm run build --prefix $(CLIENT_PATH)
	npm test -- --watchAll=false --prefix $(CLIENT_PATH)

.PHONY: all api client
