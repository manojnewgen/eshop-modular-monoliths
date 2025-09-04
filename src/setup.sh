#!/bin/bash

# Setup script for eShop Modular Monolith
# This script helps you set up the environment securely

echo "?? Setting up eShop Modular Monolith Environment"
echo "================================================"

# Check if .env already exists
if [ -f ".env" ]; then
    echo "??  .env file already exists!"
    read -p "Do you want to overwrite it? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "? Setup cancelled"
        exit 1
    fi
fi

# Copy template to .env
echo "?? Copying .env.template to .env..."
cp .env.template .env

# Generate secure passwords
echo "?? Generating secure passwords..."

# Generate random passwords
POSTGRES_PASSWORD=$(openssl rand -base64 32 | tr -d "=+/" | cut -c1-25)
PGADMIN_PASSWORD=$(openssl rand -base64 32 | tr -d "=+/" | cut -c1-25)

# Update .env file with generated passwords
if command -v sed &> /dev/null; then
    # Use sed to replace placeholder passwords
    sed -i.bak "s/your_secure_password_here/$POSTGRES_PASSWORD/" .env
    sed -i.bak "s/your_pgadmin_password_here/$PGADMIN_PASSWORD/" .env
    rm .env.bak
    
    echo "? Generated secure passwords and updated .env file"
    echo ""
    echo "?? Your generated passwords:"
    echo "   PostgreSQL: $POSTGRES_PASSWORD"
    echo "   pgAdmin:    $PGADMIN_PASSWORD"
    echo ""
    echo "?? These passwords have been saved to your .env file"
else
    echo "??  Please manually edit .env file and set secure passwords:"
    echo "   POSTGRES_PASSWORD=$POSTGRES_PASSWORD"
    echo "   PGADMIN_PASSWORD=$PGADMIN_PASSWORD"
fi

# Set secure permissions
chmod 600 .env
echo "?? Set secure permissions on .env file (600 - owner read/write only)"

echo ""
echo "? Setup complete! You can now run:"
echo "   docker-compose up -d"
echo ""
echo "?? Access URLs:"
echo "   Application: http://localhost:8080"
echo "   pgAdmin:     http://localhost:5050"
echo "   Seq Logs:    http://localhost:5341"
echo ""
echo "?? Security Notes:"
echo "   - .env file is excluded from Git"
echo "   - Keep your passwords secure"
echo "   - Change passwords in production"
echo ""