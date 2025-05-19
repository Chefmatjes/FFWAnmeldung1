# Vercel Deployment Guide

This guide explains how to deploy the FFWAnmeldung application to Vercel using GitHub integration and how to properly handle sensitive configuration settings.

## Setting up Environment Variables

### 1. Create a .env.local file for local development

Create a `.env.local` file in the root directory with the following content (replace with your actual values):

```
# SMTP Configuration
EMAIL__SMTPSERVER=smtp.gmail.com
EMAIL__SMTPPORT=587
EMAIL__SENDEREMAIL=your-email@gmail.com
EMAIL__SENDERNAME=Feuerwehr Anmeldung
EMAIL__SENDERPASSWORD=your-email-password
EMAIL__DEFAULTRECIPIENTEMAIL=recipient@example.com

# CORS Configuration
CORS__ALLOWEDORIGINS__0=http://localhost:3000
CORS__ALLOWEDORIGINS__1=https://ffwanmeldung.example.com
```

> Note: Add this file to your `.gitignore` to prevent committing sensitive information.

### 2. Configure Vercel Environment Variables

1. Go to your Vercel dashboard
2. Navigate to your project
3. Go to "Settings" → "Environment Variables"
4. Add each variable from your .env.local file as a key-value pair
5. Set the appropriate environment (Production, Preview, Development)

| Key | Value | Environment |
|-----|-------|-------------|
| EMAIL__SMTPSERVER | smtp.gmail.com | All |
| EMAIL__SMTPPORT | 587 | All |
| EMAIL__SENDEREMAIL | your-email@gmail.com | All |
| EMAIL__SENDERNAME | Feuerwehr Anmeldung | All |
| EMAIL__SENDERPASSWORD | your-email-password | All |
| EMAIL__DEFAULTRECIPIENTEMAIL | recipient@example.com | All |
| CORS__ALLOWEDORIGINS__0 | https://your-production-url.vercel.app | Production |
| CORS__ALLOWEDORIGINS__1 | https://your-preview-url.vercel.app | Preview |

### 3. GitHub Repository Secrets (for GitHub Actions)

If you're using GitHub Actions for CI/CD:

1. Go to your GitHub repository
2. Navigate to "Settings" → "Secrets and variables" → "Actions"
3. Add the same environment variables you added in Vercel

## Vercel Configuration

The `vercel.json` file in your project root configures how Vercel builds and routes your application:

```json
{
  "version": 2,
  "builds": [
    {
      "src": "Server/Server.csproj",
      "use": "@vercel/dotnet"
    },
    {
      "src": "ClientApp/package.json",
      "use": "@vercel/static-build",
      "config": {
        "distDir": "build"
      }
    }
  ],
  "routes": [
    {
      "src": "/api/(.*)",
      "dest": "Server/Server.csproj"
    },
    {
      "src": "/(.*)",
      "dest": "ClientApp/$1"
    }
  ]
}
```

## Local Testing

To test your app locally with the same environment variables that will be used in Vercel:

1. Install the dotenv CLI: `npm install -g dotenv-cli`
2. Run your application with: `dotenv -e .env.local -- dotnet run --project Server/Server.csproj`

## Deployment Process

1. Commit your changes to GitHub (excluding sensitive files)
2. Connect your GitHub repository to Vercel
3. Vercel will automatically build and deploy your application
4. Your environment variables will be applied based on the deployment environment (Production, Preview)

## Troubleshooting

If you encounter issues with environment variables not being recognized:

1. Check the format of your variable names (use double underscores for nested properties)
2. Verify that Program.cs is correctly configured to read environment variables
3. Review Vercel build logs for any errors related to configuration 