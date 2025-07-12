#!/bin/bash
set -e

echo "🌐 Starting Uptime Monitor Web UI..."
echo "==================================="

# Export PATH to include .NET
export PATH="$PATH:/home/ubuntu/.dotnet"

# Ensure required directories exist
mkdir -p /tmp/UptimeYo
mkdir -p /tmp/UptimeYo/Images
mkdir -p /tmp/UptimeYo/Logs

echo "📁 Created required directories"
echo "📊 Database: /tmp/UptimeYo/UptimeYo.db"
echo ""

# Run the web UI
echo "🎨 Starting web UI..."
echo "📍 Open your browser to: http://localhost:5000"
echo "🔒 HTTPS URL: https://localhost:5001"
echo ""

cd src/UptimeUI
dotnet run