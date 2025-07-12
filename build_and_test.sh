#!/bin/bash
set -e

echo "🔄 Building and testing Uptime Monitor..."
echo "========================================"

# Export PATH to include .NET
export PATH="$PATH:/home/ubuntu/.dotnet"

# Clean and build the solution
echo "📦 Cleaning and building solution..."
dotnet clean
dotnet build

# Run tests
echo "🧪 Running unit tests..."
dotnet test

echo "✅ Build and test completed successfully!"
echo ""
echo "To run the components:"
echo "  - Monitoring Service: ./run_service.sh"
echo "  - Web UI: ./run_ui.sh"