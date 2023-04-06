using Application;
using Application.Todos;
using DotNet.Testcontainers.Containers;
using Infrastructure.EntityFramework;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace Presentation.MinimalApi.net7.CleanArchitecture.Tests.Integration.Setup;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    //as an option for generating random port, but better is shown bellow
    //https://dotnet.testcontainers.org/api/create_docker_network/
    //private int Port = Random.Shared.Next(10000, 60000);

    private readonly DockerContainer _container = new MsSqlBuilder()
        .WithPassword("YourStrong!Passw0rd")
        .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
        .WithPortBinding(1433, true)
        .WithCleanUp(true)
        .Build();

    public IntegrationTestFactory()
    {
        //_container = new ContainerBuilder<DockerContainer>()
        //    .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
        //    .WithEnvironment("ACCEPT_EULA", "Y")
        //    .WithEnvironment("SA_PASSWORD", "YourStrong!Passw0rd")
        //    .WithEnvironment("DATABASE", "TodoMinimal7")
        //    .WithPortBinding(Port, 1433)
        //    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
        //    .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var Port = _container.GetMappedPublicPort(1433);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<TDbContext>();
            services.RemoveAll(typeof(ITodoRepository));
            services.AddDbContext<TDbContext>(options =>
            {
                options.UseSqlServer(
                        $"Server=127.0.0.1,{Port};Database=TodoMinimal7;User=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;",
                        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                    .EnableSensitiveDataLogging(false);
                //options.UseSqlServer(
                //        $"Server=127.0.0.1,{Port};Database=TodoMinimal7;User=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;",
                //        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                //    .EnableSensitiveDataLogging(false);
            });
            services.AddScoped<ITodoRepository, TodoEfCoreRepository>();
            services.EnsureDbCreated<TDbContext>();
        });
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}