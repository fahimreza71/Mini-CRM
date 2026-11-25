// tests/MiniCrm.IntegrationTests/SharedApiFixture.cs
using Xunit;

namespace MiniCrm.IntegrationTests;

// One container and one API for ALL test classes, instead of one per class.
[CollectionDefinition(Name)]
public sealed class SharedApiFixture : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}