# Products API

A RESTful API for managing product data with authentication, database integration, and event publishing capabilities.

## Features

- **Product Management**: Full CRUD operations for product data
- **Authentication**: Supports both Azure AD (production) and JWT (development)
- **Database Integration**: Uses Entity Framework Core with SQL Server
- **Event Publishing**: Supports both real and mock event publishing
- **Health Checks**: Includes database connectivity monitoring
- **API Documentation**: Swagger UI with JWT authentication support
- **CORS Support**: Configurable allowed origins

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication / Azure AD Authentication
- Swagger / OpenAPI
- Application Insights

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server (local or Azure SQL Database)
- Visual Studio 2022, VS Code, or other compatible IDE


## Project Structure

- **Controllers**: API endpoints for product management
- **Models**: Data models including Product entity
- **Data**: Database context and configuration
- **Repositories**: Data access layer with CRUD operations
- **Services**: Business logic layer
- **Events**: Event publishing functionality

## Navigation Guide
For developers starting with this codebase:

- The ASP.NET 'Products' Web API source code is available under the folder /src/Products.API/
- Start by reviewing the Program.cs file to understand the overall application setup
- Compile and run source code using the instructions in the Running Locally section
- Integration and Unit tests are included with a set of BDD-based integration tests in the test projects
- The authentication flow differs based on the environment:
- For local development, use the /obtainToken endpoint with test credentials
- For production, integrate with Azure AD as described in the configuration section

## Architecture Diagrams
Two sets of simple architecture diagrams are available showing how this Products service could form part of a distributed or microservices event-driven architecture with a few other components shown (e.g. orders, payments).
These diagrams can be found in:

    /diagram/

## Configuration

The application uses the standard ASP.NET Core configuration system. Key settings can be configured in `appsettings.json` or through environment variables:

### Connection Strings

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server;Database=ProductsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
### Authentication settings

#### For Azure AD (Production):
json"UseAzure": true,
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "yourdomain.onmicrosoft.com",
  "TenantId": "your-tenant-id",
  "ClientId": "your-client-id",
  "Audience": "your-audience"
}


#### For JWT (Development/Local):
json"UseAzure": false,
"Jwt": {
  "Key": "your-secret-key-at-least-16-characters-long",
  "Issuer": "your-issuer",
  "Audience": "your-audience"
}

#### CORS Settings
json"AllowedOrigins": [
  "https://yourfrontend.com",
  "http://localhost:3000"
]

## Running Locally

Clone the repository
Run the following commands:

bashdotnet restore
dotnet build
dotnet run

Navigate to https://localhost:5001/swagger to view the API documentation


### Authentication
The API includes a built-in authentication controller that provides JWT tokens for development purposes.
-> Auth Controller Endpoints

POST /obtainToken: Generates a JWT token for the test user

Request Body: { "username": "admin", "password": "password" }
Response: { "token": "your-jwt-token" }

The endpoint will return a JWT token if credentials are valid
In Swagger UI, click the "Authorize" button and enter your token as Bearer {your-token}
All subsequent requests will include the token


Note: The hardcoded credentials are for development purposes only. In a production environment, proper authentication should be implemented.

## Database Initialization
### Running Locally
The application automatically creates and seeds the database with sample data on startup if no products exist:

Product 1: Red, SKU001, $19.99, 100 in stock
Product 2: Blue, SKU002, $29.99, 50 in stock
Product 3: Red, SKU003, $39.99, 75 in stock

## API Endpoints
Products Controller
GET /api/products

Description: Retrieve all products
Authentication: Required
Response: 200 OK with array of product objects

GET /api/products/getproductbyID

Description: Retrieve a specific product by ID
Authentication: Required
Parameters: id (path parameter)
Response: 200 OK with product object or 404 Not Found

POST /api/products/createProduct

Description: Create a new product
Authentication: Required
Request Body: Product object without ID
Response: 201 Created with the created product including ID

PUT /api/products/updateProduct

Description: Update an existing product
Authentication: Required
Parameters: id (path parameter)
Request Body: Updated product object
Response: 200 OK with updated product or 404 Not Found

DELETE /api/products/deleteProductbyId

Description: Delete a product
Authentication: Required
Parameters: id (path parameter)
Response: 204 No Content or 404 Not Found

GET /api/products/getProductbyColor

Description: Filter products by color
Authentication: Required
Parameters: color (path parameter)
Response: 200 OK with filtered array of product objects

## Request and Response Examples

### GET /api/products
Response: 200 OK
[
  {
    "id": 1,
    "name": "Product 1",
    "description": "Description for Product 1",
    "price": 19.99,
    "color": "Red",
    "sku": "SKU001",
    "stockQuantity": 100
  },
  {
    "id": 2,
    "name": "Product 2",
    "description": "Description for Product 2",
    "price": 29.99,
    "color": "Blue",
    "sku": "SKU002",
    "stockQuantity": 50
  }
]

### POST /api/products
Request:
{
  "name": "New Product",
  "description": "Description for New Product",
  "price": 49.99,
  "color": "Green",
  "sku": "SKU004",
  "stockQuantity": 25
}

Response: 201 Created
{
  "id": 4,
  "name": "New Product",
  "description": "Description for New Product",
  "price": 49.99,
  "color": "Green",
  "sku": "SKU004",
  "stockQuantity": 25
}

## Health Checks

The API provides a health endpoint at `/health` that returns:
- 200 OK: When the application and database are healthy
- 200 OK: When the application is degraded but still functioning
- 503 Service Unavailable: When the application is unhealthy

This endpoint is anonymously accessible and doesn't require authentication.

## Deployment

The application is designed to be deployed to Azure App Service with the following components:

Azure SQL Database for data storage
Azure Front Door for global distribution and WAF protection
Application Insights for monitoring and diagnostics

This implementation takes a holistic approach to the requirements along with the job specification, providing extra layers of Azure cloud implementation that go beyond the basic requirements. The architecture considers production-grade concerns such as:

Security through Azure Front Door and WAF
Global availability and performance
Infrastructure as Code (IaC) through Bicep templates
Comprehensive monitoring with Application Insights
Environment-specific authentication mechanisms
High availability and scalability

## Note: 
The Bicep templates included in this repository are provided to give an overall idea of how infrastructure deployment would be implemented in a production environment, including the Azure setup and integration with Products Web API service. They serve as reference examples to explain the Azure Infrastructure as Code (IaC) deployment approach rather than for direct use in production.
