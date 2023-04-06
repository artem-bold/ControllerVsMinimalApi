using Infrastructure.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.MinimalApi.net7.CleanArchitecture.Tests.Integration.Setup;

public class IntegrationTestBase : IClassFixture<IntegrationTestFactory<Program, ApplicationDbContext>>
{
    public readonly IntegrationTestFactory<Program, ApplicationDbContext> Factory;
    public readonly ApplicationDbContext DbContext;

    public IntegrationTestBase(IntegrationTestFactory<Program, ApplicationDbContext> factory)
    {
        Factory = factory;
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}