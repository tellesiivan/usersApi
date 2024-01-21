# User Management API
This is a sample API implementation that allows for basic user management and retrieval of user job info and salary. It is built using ASP.NET Core Web API and uses Entity Framework Core and SQL Server for database persistence.

## Technologies Used
- ASP.NET Core Web API 
- Entity Framework Core
- SQL Server and Azure Data Studio
- JWT Authentication and Authorization
- Repository pattern
- Docker & docker-compose for containerization
  
## Features
The API supports the following endpoints and features:

- GET Users - Retrieve a list of all users
- GET User - Get details about a specific user like job title and salary
- POST User - Create a new user record
- DELETE User - Delete an existing user
- Authentication is implemented with JWT tokens issued upon user creation or login. App uses JWT middleware for authorization and access control.

The persistence layer uses Entity Framework Core code first approach with a repository pattern implementation for the domain logic. Database schema is in the SQL Server project using Azure Data Studio for queries.
