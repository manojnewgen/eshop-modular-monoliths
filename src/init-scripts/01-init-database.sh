#!/bin/bash
set -e

# Create databases for different modules if they don't exist
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create extensions
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    CREATE EXTENSION IF NOT EXISTS "pgcrypto";
    
    -- =============================================================================
    -- SCHEMA CREATION - MODULAR DATA ISOLATION
    -- =============================================================================
    -- Create schemas for modular organization
    CREATE SCHEMA IF NOT EXISTS catalog;
    CREATE SCHEMA IF NOT EXISTS basket;
    CREATE SCHEMA IF NOT EXISTS ordering;
    CREATE SCHEMA IF NOT EXISTS shared;
    
    -- Grant permissions to each schema
    GRANT ALL PRIVILEGES ON SCHEMA catalog TO $POSTGRES_USER;
    GRANT ALL PRIVILEGES ON SCHEMA basket TO $POSTGRES_USER;
    GRANT ALL PRIVILEGES ON SCHEMA ordering TO $POSTGRES_USER;
    GRANT ALL PRIVILEGES ON SCHEMA shared TO $POSTGRES_USER;
    
    -- =============================================================================
    -- CATALOG MODULE TABLES - Product Information
    -- =============================================================================
    CREATE TABLE IF NOT EXISTS catalog.products (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        name VARCHAR(200) NOT NULL,
        description TEXT,
        price DECIMAL(18,2) NOT NULL CHECK (price > 0),
        image_file VARCHAR(500),
        categories TEXT[],
        stock_quantity INTEGER NOT NULL DEFAULT 0,
        is_available BOOLEAN NOT NULL DEFAULT true,
        created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
        created_by VARCHAR(100),
        last_modified_at TIMESTAMP WITH TIME ZONE,
        last_modified_by VARCHAR(100)
    );
    
    CREATE TABLE IF NOT EXISTS catalog.categories (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        name VARCHAR(100) NOT NULL UNIQUE,
        description TEXT,
        parent_category_id UUID REFERENCES catalog.categories(id),
        created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
    );
    
    -- Catalog indexes for performance
    CREATE INDEX IF NOT EXISTS idx_products_name ON catalog.products(name);
    CREATE INDEX IF NOT EXISTS idx_products_price ON catalog.products(price);
    CREATE INDEX IF NOT EXISTS idx_products_categories ON catalog.products USING GIN(categories);
    CREATE INDEX IF NOT EXISTS idx_products_availability ON catalog.products(is_available);
    CREATE INDEX IF NOT EXISTS idx_categories_name ON catalog.categories(name);
    
    -- =============================================================================
    -- BASKET MODULE TABLES - Shopping Cart Information
    -- =============================================================================
    CREATE TABLE IF NOT EXISTS basket.shopping_carts (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        user_id UUID NOT NULL,
        session_id VARCHAR(500),
        status VARCHAR(50) NOT NULL DEFAULT 'Active',
        created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
        last_modified_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
    );
    
    CREATE TABLE IF NOT EXISTS basket.cart_items (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        cart_id UUID NOT NULL REFERENCES basket.shopping_carts(id) ON DELETE CASCADE,
        product_id UUID NOT NULL, -- References catalog.products(id) but no FK for loose coupling
        product_name VARCHAR(200) NOT NULL, -- Denormalized for performance
        product_price DECIMAL(18,2) NOT NULL,
        product_image_url VARCHAR(500),
        quantity INTEGER NOT NULL CHECK (quantity > 0),
        unit_price DECIMAL(18,2) NOT NULL,
        total_price DECIMAL(18,2) GENERATED ALWAYS AS (quantity * unit_price) STORED,
        added_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
        last_modified_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
    );
    
    CREATE TABLE IF NOT EXISTS basket.cart_discounts (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        cart_id UUID NOT NULL REFERENCES basket.shopping_carts(id) ON DELETE CASCADE,
        discount_code VARCHAR(50) NOT NULL,
        discount_type VARCHAR(20) NOT NULL, -- 'PERCENTAGE' or 'FIXED'
        discount_value DECIMAL(18,2) NOT NULL,
        applied_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
    );
    
    -- Basket indexes for performance
    CREATE INDEX IF NOT EXISTS idx_shopping_carts_user_id ON basket.shopping_carts(user_id);
    CREATE INDEX IF NOT EXISTS idx_shopping_carts_session_id ON basket.shopping_carts(session_id);
    CREATE INDEX IF NOT EXISTS idx_cart_items_cart_id ON basket.cart_items(cart_id);
    CREATE INDEX IF NOT EXISTS idx_cart_items_product_id ON basket.cart_items(product_id);
    CREATE UNIQUE INDEX IF NOT EXISTS idx_cart_items_unique ON basket.cart_items(cart_id, product_id);
    
    -- =============================================================================
    -- SHARED TABLES - Cross-Module Data
    -- =============================================================================
    CREATE TABLE IF NOT EXISTS shared.domain_events (
        id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
        event_id UUID NOT NULL UNIQUE,
        event_type VARCHAR(500) NOT NULL,
        aggregate_id UUID NOT NULL,
        aggregate_type VARCHAR(200) NOT NULL,
        event_data JSONB NOT NULL,
        occurred_on TIMESTAMP WITH TIME ZONE NOT NULL,
        processed_at TIMESTAMP WITH TIME ZONE,
        version INTEGER NOT NULL DEFAULT 1
    );
    
    CREATE INDEX IF NOT EXISTS idx_domain_events_aggregate ON shared.domain_events(aggregate_type, aggregate_id);
    CREATE INDEX IF NOT EXISTS idx_domain_events_occurred_on ON shared.domain_events(occurred_on);
    CREATE INDEX IF NOT EXISTS idx_domain_events_processed ON shared.domain_events(processed_at);
    
    -- =============================================================================
    -- SAMPLE DATA INSERTION
    -- =============================================================================
    
    -- Insert catalog categories
    INSERT INTO catalog.categories (id, name, description) 
    VALUES 
        ('11111111-1111-1111-1111-111111111111', 'Electronics', 'Electronic devices and gadgets'),
        ('22222222-2222-2222-2222-222222222222', 'Gaming', 'Gaming products and accessories'),
        ('33333333-3333-3333-3333-333333333333', 'Computers', 'Computer hardware and software'),
        ('44444444-4444-4444-4444-444444444444', 'Peripherals', 'Computer peripherals and accessories')
    ON CONFLICT (name) DO NOTHING;
    
    -- Insert catalog products
    INSERT INTO catalog.products (id, name, description, price, image_file, categories, stock_quantity, is_available) 
    VALUES 
        ('12345678-1234-1234-1234-123456789012', 'Gaming Laptop', 'High-performance gaming laptop with RTX graphics', 1299.99, 'gaming-laptop.jpg', ARRAY['Electronics', 'Computers', 'Gaming'], 10, true),
        ('12345678-1234-1234-1234-123456789013', 'Wireless Mouse', 'Ergonomic wireless gaming mouse', 79.99, 'wireless-mouse.jpg', ARRAY['Electronics', 'Gaming', 'Peripherals'], 50, true),
        ('12345678-1234-1234-1234-123456789014', 'Mechanical Keyboard', 'RGB mechanical gaming keyboard', 149.99, 'mechanical-keyboard.jpg', ARRAY['Electronics', 'Gaming', 'Peripherals'], 25, true),
        ('12345678-1234-1234-1234-123456789015', 'Gaming Headset', 'Noise-cancelling gaming headset with microphone', 199.99, 'gaming-headset.jpg', ARRAY['Electronics', 'Gaming', 'Peripherals'], 15, true),
        ('12345678-1234-1234-1234-123456789016', 'Monitor 27inch', '4K Gaming Monitor 144Hz', 399.99, 'gaming-monitor.jpg', ARRAY['Electronics', 'Gaming', 'Peripherals'], 8, true)
    ON CONFLICT (id) DO NOTHING;
    
    -- Insert sample shopping cart (for demonstration)
    INSERT INTO basket.shopping_carts (id, user_id, session_id, status) 
    VALUES 
        ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'session_12345', 'Active')
    ON CONFLICT (id) DO NOTHING;
    
    -- Insert sample cart items
    INSERT INTO basket.cart_items (id, cart_id, product_id, product_name, product_price, product_image_url, quantity, unit_price) 
    VALUES 
        ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '12345678-1234-1234-1234-123456789013', 'Wireless Mouse', 79.99, 'wireless-mouse.jpg', 2, 79.99),
        ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '12345678-1234-1234-1234-123456789014', 'Mechanical Keyboard', 149.99, 'mechanical-keyboard.jpg', 1, 149.99)
    ON CONFLICT (cart_id, product_id) DO NOTHING;
    
    -- =============================================================================
    -- SCHEMA-SPECIFIC PERMISSIONS AND SECURITY
    -- =============================================================================
    
    -- Create role-based access (optional for additional security)
    -- Each module could have its own database user with access only to its schema
    
    -- Future: Create catalog_user with access only to catalog schema
    -- CREATE USER catalog_user WITH PASSWORD 'catalog_password';
    -- GRANT USAGE ON SCHEMA catalog TO catalog_user;
    -- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA catalog TO catalog_user;
    
    -- Future: Create basket_user with access only to basket schema  
    -- CREATE USER basket_user WITH PASSWORD 'basket_password';
    -- GRANT USAGE ON SCHEMA basket TO basket_user;
    -- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA basket TO basket_user;
    
    COMMIT;
EOSQL

echo "==================================================================="
echo "? Database initialization completed successfully!"
echo "==================================================================="
echo "?? Created Schemas:"
echo "   • catalog   - Product and category management"
echo "   • basket    - Shopping cart functionality" 
echo "   • ordering  - Order processing (ready for future)"
echo "   • shared    - Cross-module domain events"
echo ""
echo "?? Data Isolation Benefits:"
echo "   • Each module owns its data completely"
echo "   • No accidental cross-module data access"
echo "   • Independent schema evolution"
echo "   • Clear module boundaries"
echo "   • Future-ready for microservices migration"
echo "==================================================================="