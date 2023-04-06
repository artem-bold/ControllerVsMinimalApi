using System.Net;
using System.Net.Http.Json;
using Application.Todos.CreateTodo;
using Bogus;
using FluentAssertions;
using Infrastructure.EntityFramework;
using Presentation.MinimalApi.net7.CleanArchitecture.Tests.Integration.Setup;

namespace Presentation.MinimalApi.net7.CleanArchitecture.Tests.Integration.TodosController;

public class CreateTodoControllerTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    private readonly Faker<CreateTodoCommand> _faker = new Faker<CreateTodoCommand>()
        .CustomInstantiator(x => new CreateTodoCommand(
            x.Lorem.Sentence(1, 3),
            x.Lorem.Sentences(1, " ")));

    public CreateTodoControllerTests(IntegrationTestFactory<Program, ApplicationDbContext> factory) : base(factory)
    {
        Randomizer.Seed = new Random(5425479);
        _client = Factory.CreateClient();
    }

    [Fact]
    public async Task CreateTodo_CreatesTodo_WhenDataIsValid()
    {
        //Arrange
        var createTodoCommand = _faker.Generate();

        //Act
        var response = await _client.PostAsJsonAsync("/todos", createTodoCommand);

        //Assert
        var todoResponse = await response.Content.ReadFromJsonAsync<CreateTodoCommandResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should()
            .Be($"http://localhost/todos/{todoResponse!.TodoId}");
    }

    [Fact]
    public async Task CreateTodo_ReturnsValidationError_WhenDataIsInvalid()
    {
        //Arrange
        var createTodoCommand = new CreateTodoCommand("", "");

        //Act
        var response = await _client.PostAsJsonAsync("/todos", createTodoCommand);

        //Assert
        var todoResponse = await response.Content.ReadFromJsonAsync<CreateTodoCommandResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}