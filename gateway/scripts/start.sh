#!/bin/bash

# Build and start all services including gateway

echo "🚀 Starting SOA Project with API Gateway..."

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo "❌ Docker is not running. Please start Docker first."
        exit 1
    fi
    echo "✅ Docker is running"
}

# Function to stop existing containers
cleanup() {
    echo "🧹 Cleaning up existing containers..."
    docker-compose down --remove-orphans
}

# Function to build and start services
start_services() {
    echo "🔨 Building and starting services..."
    docker-compose up --build -d
    
    if [ $? -eq 0 ]; then
        echo "✅ All services started successfully!"
        echo ""
        echo "📋 Service URLs:"
        echo "🌐 API Gateway: http://localhost:3000"
        echo "📊 Health Check: http://localhost:3000/health"
        echo "🔍 Service Discovery: http://localhost:3000/services"
        echo ""
        echo "🎯 Microservices (via Gateway):"
        echo "📝 Blogs: http://localhost:3000/api/blogs"
        echo "👥 Stakeholders: http://localhost:3000/api/stakeholders"
        echo "🚌 Tours: http://localhost:3000/api/tours"
        echo "👫 Followings: http://localhost:3000/api/followings"
        echo ""
        echo "📺 Frontend: http://localhost:5173"
    else
        echo "❌ Failed to start services"
        exit 1
    fi
}

# Function to show service status
show_status() {
    echo "📊 Service Status:"
    docker-compose ps
}

# Main execution
check_docker
cleanup
start_services
show_status

echo ""
echo "🎉 SOA Project is now running!"
echo "Use 'docker-compose logs -f' to view logs"
echo "Use 'docker-compose down' to stop all services"
