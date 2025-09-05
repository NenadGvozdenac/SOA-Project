# Use .NET 8.0 SDK as base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy proto files first (needed for gRPC)
COPY proto ./proto

# Copy project files and restore dependencies
COPY services/tours_service/*.csproj ./
RUN dotnet restore

# Copy the rest of the app
COPY services/tours_service ./
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published app
COPY --from=build /app/out ./

# Remove ASPNETCORE_URLS to use appsettings.Development.json configuration
# This allows both HTTP (8082) and gRPC (8083) endpoints
# ENV ASPNETCORE_URLS=http://+:8082;http://+:8083
# Expose both ports
EXPOSE 8082 8083

# Run the app
CMD ["dotnet", "tours_service.dll"]
