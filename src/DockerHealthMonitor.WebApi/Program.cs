using Docker.DotNet;
using Docker.DotNet.Models;
using System.Net.Sockets;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dockerUri = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? "npipe://./pipe/docker_engine"
    : "unix:///var/run/docker.sock";

app.MapGet("/health/{containerName}", async (string containerName) =>
{
    try
    {
        using var dockerClient = new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();

        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });

        var matchingContainer = containers.FirstOrDefault(c =>
            c.Names.Any(n => n.TrimStart('/') == containerName));

        if (matchingContainer == null)
            return Results.NotFound($"Container '{containerName}' not found.");

        var containerDetails = await dockerClient.Containers.InspectContainerAsync(matchingContainer.ID);

        if (containerDetails.State.Health != null)
        {
            return Results.Ok(new
            {
                Container = containerName,
                HealthStatus = containerDetails.State.Health.Status, // e.g., "healthy", "unhealthy", or "starting"
                Details = containerDetails.State.Health
            });
        }
        else
        {
            // If no health check is defined, fall back to the overall container status.
            return Results.Ok(new
            {
                Container = containerName,
                Status = containerDetails.State.Status,  // e.g., "running", "exited", etc.
                Message = "No explicit health check configured for this container."
            });
        }
    }
    catch (HttpRequestException ex)
    {
        if ((ex.InnerException as SocketException)?.SocketErrorCode == SocketError.AccessDenied)
        {
            return Results.Problem(" It appears that the application does not have permission to access the Docker socket. " +
                "Ensure that the container's user has read/write access to /var/run/docker.sock. " +
                "For example, you can run the container as root (user 0:0) or add your non-root user to the docker group on your host.");
        }
        else
        {
            return Results.Problem($"Error retrieving container health: {ex.Message}");
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving container health: {ex.Message}");
    }
});

app.Run();
