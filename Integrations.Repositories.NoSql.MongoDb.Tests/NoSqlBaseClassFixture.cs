using System;
using NoSql.MongoDb.Tests.Helpers;

namespace NoSql.MongoDb.Tests;

public class NoSqlBaseClassFixture : IDisposable
{
    public NoSqlBaseClassFixture()
    {
        DockerHelper.StartContainer();
    }

    public void Dispose()
    {
        DockerHelper.StopContainer();
        DockerHelper.RemoveContainer();
    }
}

