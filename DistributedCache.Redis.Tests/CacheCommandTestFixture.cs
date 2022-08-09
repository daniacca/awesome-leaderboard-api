using System;
using Common.DistribuitedCache.Tests.Helpers;

namespace Common.DistributedCache.Tests;

public class CacheCommandTestFixture : IDisposable
{
    public CacheCommandTestFixture()
    {
        DockerHelper.StartContainer();
    }

    public void Dispose()
    {
        DockerHelper.StopContainer();
        DockerHelper.RemoveContainer();
    }
}

