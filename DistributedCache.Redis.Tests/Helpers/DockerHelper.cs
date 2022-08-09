using System;
using System.Collections.Generic;
using System.Threading;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Common.DistribuitedCache.Tests.Helpers;

public static class DockerHelper
{
    public static string RunningContainerID { get; private set; } = null;

    private static bool IsRunningOnWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

    private static DockerClient GetDockerClient()
    {
        var dockerUri = IsRunningOnWindows
            ? "npipe://./pipe/docker_engine"
            : "unix:///var/run/docker.sock";
        return new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
    }

    public static void StartContainer()
    {
        // Load and launch a mongo DB container for running test:
        var client = GetDockerClient();

        client.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "redis",
            Tag = "latest"
        }, null, new Progress<JSONMessage>()).Wait();

        var response = client.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = "redis",
            Name = "redis-unit-test",
            ExposedPorts = new Dictionary<string, EmptyStruct>() { { 6379.ToString(), new EmptyStruct() } },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            6379.ToString(),
                            new List<PortBinding>
                            {
                                new PortBinding { HostPort = 6379.ToString() }
                            }
                        }
                    }
            }
        }).Result;

        var running = client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters()).Result;
        if(running)
            RunningContainerID = response.ID;
    }

    public static bool StopContainer()
    {
        var client = GetDockerClient();
        return client.Containers.StopContainerAsync(
            RunningContainerID,
            new ContainerStopParameters { WaitBeforeKillSeconds = 5 },
            CancellationToken.None).Result;
    }

    public static void RemoveContainer()
    {
        var client = GetDockerClient();
        client.Containers.RemoveContainerAsync(
            RunningContainerID,
            new ContainerRemoveParameters { Force = true },
            CancellationToken.None).Wait();
    }
}

