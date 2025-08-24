@echo off
REM Build and start all services including gateway

echo 🚀 Starting SOA Project with API Gateway...

REM Check if Docker is running
docker info >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker is not running. Please start Docker first.
    exit /b 1
)
echo ✅ Docker is running

REM Clean up existing containers
echo 🧹 Cleaning up existing containers...
docker-compose down --remove-orphans

REM Build and start services
echo 🔨 Building and starting services...
docker-compose up --build -d

if %ERRORLEVEL% EQU 0 (
    echo ✅ All services started successfully!
    echo.
    echo 📋 Service URLs:
    echo 🌐 API Gateway: http://localhost:3000
    echo 📊 Health Check: http://localhost:3000/health
    echo 🔍 Service Discovery: http://localhost:3000/services
    echo.
    echo 🎯 Microservices (via Gateway):
    echo 📝 Blogs: http://localhost:3000/api/blogs
    echo 👥 Stakeholders: http://localhost:3000/api/stakeholders
    echo 🚌 Tours: http://localhost:3000/api/tours
    echo 👫 Followings: http://localhost:3000/api/followings
    echo.
    echo 📺 Frontend: http://localhost:5173
    echo.
    echo 📊 Service Status:
    docker-compose ps
    echo.
    echo 🎉 SOA Project is now running!
    echo Use 'docker-compose logs -f' to view logs
    echo Use 'docker-compose down' to stop all services
) else (
    echo ❌ Failed to start services
    exit /b 1
)
