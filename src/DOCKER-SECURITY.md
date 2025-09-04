# ?? Secure Docker Configuration

## Why Environment Variables?

The `docker-compose.yml` file is committed to Git, so it **should never contain secrets**. Instead, we use environment variables that are loaded from `.env` files that are **not tracked by Git**.

## Quick Setup

### 1. Create Your Environment File
```bash
# Copy the template
cp .env.template .env

# Edit with your actual passwords
nano .env
```

### 2. Set Your Passwords
Edit `.env` and replace placeholder values:
```bash
POSTGRES_PASSWORD=YourSecurePassword123!
PGADMIN_PASSWORD=YourPgAdminPassword123!
```

### 3. Run Docker Compose
```bash
docker-compose up -d
```

## Security Benefits

? **No secrets in Git** - Passwords are not committed to version control  
? **Environment-specific** - Different passwords for dev/staging/production  
? **Easy rotation** - Change passwords without touching docker-compose.yml  
? **Team-friendly** - Each developer uses their own .env file  

## File Security

| File | Status | Contains |
|------|--------|----------|
| `docker-compose.yml` | ? **Tracked in Git** | No secrets, uses `${VARIABLES}` |
| `.env.template` | ? **Tracked in Git** | Example values only |
| `.env` | ? **NOT tracked** | Your actual passwords |
| `.env.local` | ? **NOT tracked** | Local overrides |
| `.env.production` | ? **NOT tracked** | Production secrets |

## Production Security

For production environments, consider:

### 1. Docker Secrets (Recommended)
```yaml
services:
  eshop-db:
    environment:
      POSTGRES_PASSWORD_FILE: /run/secrets/postgres_password
    secrets:
      - postgres_password

secrets:
  postgres_password:
    external: true
```

### 2. External Secret Management
- **Azure Key Vault**
- **AWS Secrets Manager** 
- **HashiCorp Vault**
- **Kubernetes Secrets**

### 3. Strong Password Policy
- Minimum 12 characters
- Mix of letters, numbers, symbols
- Unique per environment
- Regular rotation

## Troubleshooting

### Missing .env file
```bash
Error: required variable POSTGRES_PASSWORD is not set
```
**Solution:** Copy `.env.template` to `.env` and set your passwords

### Wrong permissions
```bash
chmod 600 .env  # Only owner can read/write
```

### Verify environment variables
```bash
docker-compose config  # Shows final configuration with variables resolved
```

## What Changed?

### Before (? Insecure)
```yaml
environment:
  POSTGRES_PASSWORD: eshoppass123  # Exposed in Git!
```

### After (? Secure)
```yaml
environment:
  POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}  # From .env file
```

This ensures your secrets stay secret! ??