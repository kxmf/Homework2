using Xunit;

namespace Homework2.IntegrationTests.Fixtures;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresDatabaseFixture>
{
}
