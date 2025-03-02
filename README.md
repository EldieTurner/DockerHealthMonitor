
# DockerHealthMonitor

DockerHealthMonitor is a .NET 8.0 Web API that inspects and monitors Docker containers on the host. The API is containerized, supports multi-architecture (x64 and ARM64), and provides endpoints to retrieve container details and health status. It also integrates health checks to report the overall application status.

## Features

- **Multi-Architecture Support:**  
  Build and run on both x64 and ARM64 systems (e.g., Intel-based systems and Raspberry Pi).

- **Container Inspection:**  
  - **GET `/containers`**: Lists all Docker containers on the host, including their ID, name, state, and (if available) health status.
  - **GET `/health/{containerName}`**: Returns detailed health information for the specified container.

- **Application Health Checks:**  
  The API registers a health endpoint (`/health`) that Docker can use to monitor the app's health.

- **Swagger/OpenAPI Documentation:**  
  Interactive API documentation is available via Swagger.

- **Logging:**  
  Console logging is configured to capture startup events and critical errors.

## Endpoints

### GET `/containers`

Returns a list of all Docker containers on the host with key details:
- **Id**
- **Name**
- **State**
- **HealthStatus** (if available)

### GET `/health/{containerName}`

Returns the health status (or running status, if health information is not available) of a specified container. The response structure is similar to:

```json
{
  "Container": "containerName",
  "HealthStatus": "healthy", 
  "Details": { ... }
}
```

### GET `/health`

Returns the overall health status of the application. Docker can use this endpoint to monitor the container's health.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/)
- (Optional) [Docker Buildx](https://docs.docker.com/buildx/working-with-buildx/) for multi-architecture builds

## Building and Running

### Running Locally

1. **Clone the repository:**

   ```bash
   git clone https://github.com/yourusername/DockerHealthMonitor.git
   cd DockerHealthMonitor/DockerHealthMonitor.WebApi
   ```

2. **Run the API locally:**

   ```bash
   dotnet run
   ```

3. **Access Swagger:**  
   Open your browser and navigate to `http://localhost:5000/swagger` (or the port configured).

### Docker Build and Run

The project is containerized with a multi-stage Dockerfile.

#### Building a Multi-Architecture Image

Use Docker Buildx to build and push the image for both ARM64 and x64:

```bash
docker buildx build --platform linux/amd64,linux/arm64 -t eldieturner/dockerhealthmonitor:latest --push .
```

This command builds the image for both architectures and pushes it to Docker Hub under the `eldieturner/dockerhealthmonitor:latest` tag.

#### Running the Container with Docker Run

Ensure you mount the Docker socket if you need to inspect host containers:

```bash
docker run -d -p 8080:8080 -p 8081:8081 -v /var/run/docker.sock:/var/run/docker.sock eldieturner/dockerhealthmonitor:latest
```

- **Port Mapping:**  
  The container exposes port `8080` (HTTP) and `8081` (HTTPS). Adjust the ports as needed.

- **Health Check in Docker:**  
  The application exposes a `/health` endpoint.

## Docker Compose

You can use a `docker-compose.yml` file to manage the container. Below is an example that includes a Docker healthcheck:

```yaml
version: '3.8'

services:
  dockerhealthmonitor:
    image: eldieturner/dockerhealthmonitor:latest
    ports:
      - "8010:8080"    # HTTP
      - "8011:8081"    # HTTPS
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 10s
```

### Running with Docker Compose

Start the container using:

```bash
docker-compose up -d
```

To view the health status, run:

```bash
docker ps
```

Docker will display the container’s health status as reported by the `/health` endpoint.

## Logging and Error Reporting

- Startup and runtime events are logged to the console.
- Critical errors during startup are caught and logged.
- Health endpoints provide a mechanism for both the app and Docker to report if any issues occur.

## License

[MIT License](LICENSE)