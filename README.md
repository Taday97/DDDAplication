# DDDAplication

**DDDAplication** is a powerful web application built with **.NET Core** that demonstrates modern architectural practices, including **Domain-Driven Design (DDD)**, **Identity**, and **Microservices**. The application supports user registration, authentication, password reset, comprehensive testing, global exception handling, and Docker support.

This project is ideal for showcasing clean architecture, security practices with **ASP.NET Identity**, and robust CI/CD pipelines.

---

### Key Features:

- **User Registration and Authentication**: Secure user registration and login using **ASP.NET Identity**, with token-based authentication for API security.
- **Password Reset**: Secure password reset using a token-based system.
- **Global Exception Handling**: Centralized error management across the application, ensuring consistent error responses.
- **API Documentation**: Easily explore the application's API through **Swagger UI**.
- **Docker Support**: The application and **SQL Server** database are containerized using Docker for streamlined development and deployment.
- **CI/CD Pipeline**: Automated testing, building, and deployment using GitHub Actions.
- **Improved Performance**: Repository methods optimized with `AsNoTracking()` for better performance in read-only queries.

---

### Technologies Used:

- **.NET Core**: The core framework for building cross-platform web applications.
- **ASP.NET Identity**: For user authentication, including registration, login, and password management.
- **Entity Framework Core**: For seamless data access and ORM functionality.
- **AutoMapper**: Simplifies object-to-object mapping, keeping code maintainable and clean.
- **Dependency Injection**: Implements loose coupling for better testability and scalability.
- **FluentAssertions**: A library for writing readable and expressive assertions in tests.
- **Docker**: Used to containerize both the application and the SQL Server database, ensuring a consistent environment.
- **CI/CD (GitHub Actions)**: Continuous Integration and Continuous Deployment pipeline for automated testing and deployment.
- **Swagger UI**: Auto-generated documentation for the application's API endpoints.
- **Nito.AsyncEx**: For enhancing asynchronous operations and simplifying concurrent task management.

---
## 🏗️ System Architecture

### Layered Architecture

**DDDAplication** implements a layered architecture that adheres to Domain-Driven Design (DDD) principles:
![Screenshot 2025-05-09 014556](https://github.com/user-attachments/assets/ead7d67f-6120-4955-a6ee-4622c48f3824)


---

### 📦 Infrastructure Layer
- **Repository Implementations**: Data access logic
- **AppDbContext**: Entity Framework Core configuration
- **ASP.NET Identity**: Authentication and authorization
- **JWT Service**: Token generation and validation
- **Database Migration**: Schema management

---

### 🧠 Domain Layer
- **Domain Entities**: Business objects with behavior
- **Repository Interfaces**: Abstractions for data access
- **Value Objects**: Immutable objects representing concepts with no identity

---

### ⚙️ Application Layer
- **Application Services**: Implement use cases and business workflows
- **DTOs (Data Transfer Objects)**: For API communication
- **Validators**: Input validation using FluentValidation
- **API Response Wrappers**: Standardized API response format

---

### 🌐 API Layer
Entry point for client applications, handles HTTP requests/responses.

- **Controllers**: Define endpoints and route requests to appropriate services
- **Middleware**: Handle cross-cutting concerns like authentication and exception handling
- **Program.cs**: Application configuration and service registration

---

### 🖥️ Client Applications
- **User Browsers & API Clients**: Consumers of the API

---

Each layer has specific responsibilities and promotes separation of concerns to maintain a scalable and maintainable codebase.

### Setup and Installation:

Follow these steps to set up the application on your local machine using Docker:

#### Prerequisites:

1. **Docker**: Ensure Docker is installed and running.
2. **.NET SDK**: Ensure the .NET 8.0 SDK or later is installed on your machine.

#### Steps to Run the Application:

1. Clone the repository:

    ```bash
    git clone https://github.com/Taday97/DDDAplication.git
    cd DDDAplication
    ```

2. Build and start the Docker containers for the application and database:

    ```bash
    docker-compose up --build
    ```

    This will:
    - Build and start the application.
    - Set up the SQL Server database in a Docker container.
    - Automatically connect the application to the database.

3. **Access the Application**: After the containers are running, navigate to `http://localhost:5000/swagger/index.html` to view the Swagger UI, which allows you to interact with the API.

---

### CI/CD Pipeline:

The project includes a CI/CD pipeline set up using **GitHub Actions**. This pipeline automatically:

1. **Builds and Tests** the application.
2. **Deploys** it (future configurations can be added for deployment to a server or cloud platform).

You can view the CI/CD configuration in the `.github/workflows/ci-cd.yml` file.

---

### Testing Approach:

This project includes **unit and integration tests** to ensure the quality and functionality of the application. The tests are designed to validate the core features of the application, such as user authentication and data access.

To run the tests locally:

1. Ensure that Docker containers are running (application and database).
2. Run the following command:

    ```bash
    dotnet test --configuration Release
    ```

---

### Global Exception Handling:

The application includes a robust **global exception handling** system to ensure:
- **Consistent error responses** throughout the application.
- **Logging of exceptions** for easier debugging in production environments.
- **User-friendly error messages** without exposing sensitive details.

---

### Docker Setup:

- **Dockerfile**: The file to build the application Docker image.
- **docker-compose.yml**: Defines and runs multi-container Docker applications. It includes both the application container and the SQL Server container for local development.

---

### Screenshots:

![image](https://github.com/user-attachments/assets/051f0f12-aab9-40ae-99cb-3d0b9157dfca)

---
