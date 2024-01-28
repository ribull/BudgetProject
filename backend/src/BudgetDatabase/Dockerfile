# Copy and Restore Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore  
WORKDIR /src

# Copy files
COPY . ./main

# Restore nuget packages
WORKDIR /src/main
RUN dotnet restore

# Build Stage
FROM restore AS build
RUN dotnet build -c release -o /app/build  --no-restore

# Publish Stage
FROM build as publish
RUN dotnet publish -c release -o /app/publish --no-restore

# Runtime!
FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT dotnet BudgetDatabase.dll --server $POSTGRES_SERVER --port $POSTGRES_PORT --username $POSTGRES_USERNAME --password $POSTGRES_PASSWORD