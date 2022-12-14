using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Shared.Test.Fixtures;

public class SharedContainerFixtures : IDisposable
{
    public readonly TestcontainersContainer AzureSqlContainer;
    public readonly RedisTestcontainer RedisTestcontainer;

    public SharedContainerFixtures()
    {
        using var stdOutStream = new MemoryStream();
        using var stdErrStream = new MemoryStream();
        using var consumer = Consume.RedirectStdoutAndStderrToStream(stdOutStream, stdErrStream);
        AzureSqlContainer = new TestcontainersBuilder<TestcontainersContainer>()
                            .WithName($"AZURE-SQL-{Guid.NewGuid():D}")
                            .WithImage("mcr.microsoft.com/azure-sql-edge")
                            .WithPortBinding("1433", true)
                            .WithEnvironment(new Dictionary<string, string>
                            {
                                ["ACCEPT_EULA"] = "Y",
                                ["MSSQL_SA_PASSWORD"] = "testPassword@"
                            })
                            .WithOutputConsumer(consumer)
                            .WithWaitStrategy(Wait.ForUnixContainer()
                                                  .UntilMessageIsLogged(consumer.Stdout, "EdgeTelemetry starting up"))
                            .Build();
        RedisTestcontainer = new TestcontainersBuilder<RedisTestcontainer>()
                             .WithName($"REDIS-{Guid.NewGuid():D}")
                             .WithImage("redis:7")
                             .WithPortBinding(6379, true)
                             .Build();

        var taskList = new List<Task>
        {
            AzureSqlContainer.StartAsync(),
            RedisTestcontainer.StartAsync()
        };
        Task.WhenAll(taskList).Wait();
    }

    public void Dispose()
    {
        AzureSqlContainer.DisposeAsync()
                         .GetAwaiter().GetResult();
        RedisTestcontainer.DisposeAsync()
                          .GetAwaiter().GetResult();
    }
}