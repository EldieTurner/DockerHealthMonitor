using Docker.DotNet;
using Docker.DotNet.Models;

namespace DockerHealthMonitor.WebApi.Services;

public class DockerService : IDockerService
{
    private readonly ILogger<DockerService> _logger;
    private readonly DockerClient _dockerClient;

    public DockerService(ILogger<DockerService> logger)
    {
        // Use Unix socket on Linux/Mac and named pipe on Windows.
        var dockerUri = Environment.OSVersion.Platform == PlatformID.Unix
            ? "unix:///var/run/docker.sock"
            : "npipe://./pipe/docker_engine";

        _dockerClient = new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ContainerListResponse>> GetContainersAsync()
    {
        try
        {
            var containers = await _dockerClient.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    All = true
                });
            return containers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch containers.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ContainerInspectResponse> InspectContainerAsync(string containerName)
    {
        try
        {   // Get the container by name (container names in Docker usually come with a leading slash)
            var containers = await GetContainersAsync();
            var container = containers.FirstOrDefault(c => c.Names.Any(n => n.TrimStart('/') == containerName)) ?? throw new Exception($"Container with name '{containerName}' not found.");
            return await _dockerClient.Containers.InspectContainerAsync(container.ID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inspect container: {ContainerName}", containerName);
            throw;
        }
    }
}

public interface IDockerService
{
    /// <summary>
    /// Gets a list of all Docker containers.
    /// </summary>
    /// <returns>A list of container summaries.</returns></returns>
    Task<IEnumerable<ContainerListResponse>> GetContainersAsync();
    /// <summary>
    /// Inspects a specific container by name.
    /// </summary>
    /// <param name="containerName"> The name of the container to inspect.</param>
    /// <returns> The container inspection response.</returns>
    Task<ContainerInspectResponse> InspectContainerAsync(string containerName);
}
