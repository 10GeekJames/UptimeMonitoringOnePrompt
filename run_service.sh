#!/bin/bash
set -e

echo "🚀 Starting Uptime Monitor Service..."
echo "===================================="

# Export PATH to include .NET
export PATH="$PATH:/home/ubuntu/.dotnet"

# Ensure required directories exist
mkdir -p /tmp/UptimeYo
mkdir -p /tmp/UptimeYo/Images
mkdir -p /tmp/UptimeYo/Logs

echo "📁 Created required directories"
echo "📊 Database: /tmp/UptimeYo/UptimeYo.db"
echo "🖼️  Images: /tmp/UptimeYo/Images"
echo "📝 Logs: /tmp/UptimeYo/Logs"
echo ""

# Run the monitoring service
echo "🔍 Starting monitoring service..."
cd src/UptimeService
dotnet run