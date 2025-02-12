# Docker Health Monitor

A lightweight, cross-platform .NET 6 minimal API that retrieves the health status of Docker containers running on your host machine. It leverages the Docker.DotNet library to inspect container details and returns either the container’s health (if defined) or its overall status.
## Overview

This project provides a simple HTTP API endpoint (/health/{containerName}) that:

    Queries the Docker daemon to locate a container by its name.
    Checks for a defined health check (via Docker's HEALTHCHECK instruction) and returns its status (e.g. "healthy", "unhealthy", or "starting").
    Falls back to reporting the container’s overall status (such as "running" or "exited") when no health check is configured.
    Includes exception handling with specific guidance for permission issues when accessing the Docker socket.

## Features

    Health Check Reporting: Returns detailed health status if available.
    Fallback Status: Provides container state information when no explicit health check exists.
    Cross-Platform Support: Automatically detects the Docker daemon endpoint (using Unix sockets on Linux/macOS and named pipes on Windows).
    Helpful Error Handling: Guides users to resolve common issues such as permission errors.

## Prerequisites

    .NET 8 SDK
    Docker installed and running on your host.
    Permissions to access the Docker daemon. When running in a container, ensure you mount the Docker socket (e.g., /var/run/docker.sock).

## Building and Running Locally

Clone the repository:
```
git clone <repository-url>
cd DockerHealthMonitor
```
Restore and Build the Application:
```
dotnet restore
dotnet build
```
Run the Application:
```
dotnet run
```
The API will start listening on the default URL (e.g., http://localhost:5000).

## Running in a Docker Container

```
docker pull eldieturner/dockerhealthmonitor:latest
```

## API Usage

### Endpoint
```
GET /health/{containerName}
```
Examples

Checking Health of a Container with a Healthcheck Configured:
```
curl http://localhost:5000/health/portainer
```
Possible Response:
```
{
  "Container": "portainer",
  "HealthStatus": "healthy",
  "Details": {
    "Status": "healthy",
    "FailingStreak": 0,
    "Log": [ ... ]
  }
}
```
Checking Health of a Container Without a Healthcheck:
```
{
  "Container": "portainer",
  "Status": "running",
  "Message": "No explicit health check configured for this container."
}
```
Container Not Found:
```
{
    "message": "Container 'portainer' not found."
}
```

## Troubleshooting Permissions

If you receive a "Permission Denied" error when accessing /var/run/docker.sock, consider the following:

    Running as Root: Running the container as root (e.g., using user: "0:0" in your docker-compose file) will bypass permission issues.
    Adding the User to the Docker Group: If you run containers with a non-root user (e.g., UID 1000:1000), ensure that the user is in the host’s Docker group or has equivalent permissions.
    Adjusting Socket Permissions: For development purposes only, you might change the Docker socket permissions using chmod 666 /var/run/docker.sock on your host. (Not recommended for production.)

Contributing

Contributions are welcome! Please fork the repository and submit pull requests. For major changes, please open an issue first to discuss what you would like to change.
License

This project is licensed under the MIT License. See the LICENSE file for details.