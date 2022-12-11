using Shared.Test.Fixtures;
using Xunit;

namespace Modules.Account.Test.CollectionDefinitions;

[CollectionDefinition("Container")]
public class SharedContainerCollection : ICollectionFixture<SharedContainerFixtures>
{
}