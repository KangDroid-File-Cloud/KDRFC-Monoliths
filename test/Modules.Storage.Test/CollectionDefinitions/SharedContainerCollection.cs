using Shared.Test.Fixtures;
using Xunit;

namespace Modules.Storage.Test.CollectionDefinitions;

[CollectionDefinition("Container")]
public class SharedContainerCollection : ICollectionFixture<SharedContainerFixtures>
{
}