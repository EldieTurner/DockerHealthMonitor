# Docker Health Monitor

Docker Health Monitor is a lightweight, cross‑platform API built with .NET 8 Minimal API that monitors the health of Docker containers running on your host machine. The application leverages the Docker.DotNet library to inspect container details and provides two primary endpoints: one for checking the health of a specific container and another for listing all containers.

You can view and pull the image from Docker Hub here:
[eldieturner/dockerhealthmonitor](https://hub.docker.com/r/eldieturner/dockerhealthmonitor)
## Features

    Container Health Inspection:
    Query the health status of a specific container via /health/{containerName}. If a container is configured with a Docker HEALTHCHECK, its detailed health status (e.g. "healthy", "unhealthy", or "starting") is returned; otherwise, the overall container state (e.g., "running", "exited") is provided.

    List All Containers:
    Retrieve a simplified list of all containers on the host with key details such as ID, names, status, state, and image via the /containers endpoint.

    Enhanced Logging:
    Detailed logging is implemented throughout the API to trace requests, container lookup, and error handling, which aids in debugging issues like Docker socket permission errors.

    Cross-Platform Support:
    The API automatically detects the correct Docker daemon endpoint—using Unix sockets on Linux/macOS and named pipes on Windows.

## Prerequisites

    .NET 8 SDK
    Docker (with Docker Engine running)
    Permissions to access the Docker daemon (mount /var/run/docker.sock for Linux/macOS or the named pipe for Windows)

## Installation & Usage
### Running Locally

    Clone the Repository:
```
git clone https://github.com/eldieturner/DockerHealthMonitor.git
cd DockerHealthMonitor
```
Restore and Build:
```
dotnet restore
dotnet build
```
Run the Application:
```
dotnet run --project src/DockerHealthMonitor.WebApi
```
The API will start listening on port 8080 (as configured in your Dockerfile and launch settings).

## Endpoints

### Health Check Endpoint
```
GET /health/{containerName}
```
Example:
```
curl http://localhost:8080/health/portainer
```
Response when a HEALTHCHECK is defined:
```
{
  "Container": "portainer",
  "HealthStatus": "healthy",
  "Details": { ... }
}
```
Response if no explicit health check is configured:
```
{
  "Container": "portainer",
  "Status": "running",
  "Message": "No explicit health check configured for this container."
}
```
### List All Containers Endpoint
```
GET /containers
```
Example:
```
curl http://localhost:8080/containers
```
Response:
```
[
  {
    "ID": "abc123...",
    "Names": ["/portainer", "/someothercontainer"],
    "Status": "Up 2 hours",
    "State": "running",
    "Image": "your-image"
  },
  ...
]
```
## Running with Docker
### Docker Run

To run the API in a container, ensure the Docker socket is mounted:
```
docker run -d -p 8080:80 -v /var/run/docker.sock:/var/run/docker.sock eldieturner/dockerhealthmonitor:latest
```
### Docker Compose

Create a docker-compose.yml file (if you haven’t already) with the following content:
```
version: '3.8'
services:
  dockerhealthmonitor:
    image: eldieturner/dockerhealthmonitor:latest
    container_name: dockerhealthmonitor
    ports:
      - "8080:8080"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
```
Then run:
```
docker-compose up -d dockerhealthmonitor
```

Build & Deployment
Multi-Architecture Build

The project uses Docker Buildx to build multi-architecture images for both linux/amd64 and linux/arm64. To build and push the image:

docker buildx build --platform linux/arm64,linux/amd64 -t eldieturner/dockerhealthmonitor:latest --push -f src/DockerHealthMonitor.WebApi/Dockerfile .

Dockerfile Overview

Your Dockerfile performs the following:

    Uses multi-stage builds to restore, build, and publish the .NET application.
    Sets the working directory to /app and exposes ports 8080 and 8081.
    Copies the published output into the final runtime image.

For details, see the Dockerfile.
Contributing

Contributions are welcome! Please fork the repository and create a pull request with your improvements or bug fixes. For major changes, open an issue first to discuss what you’d like to change.
License

This project is licensed under the MIT License. See the LICENSE file for details.
