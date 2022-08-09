using System;

namespace Common.DistributedCache.Tests;

internal class TestingClass
{
    public int IdTest { get; set; } = new Random().Next(1000000);
    public string NameTest { get; set; } = $"Name_Test_{Guid.NewGuid()}";

    public override bool Equals(object obj)
    {
        if (obj is not TestingClass)
            return false;

        var toCompare = obj as TestingClass;

        return IdTest == toCompare.IdTest && NameTest.Equals(toCompare.NameTest);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
