using System.Net;
using Bogus;
using FluentAssertions;
using Infrastructure.EntityFramework;
using System.Net.Http.Json;
using Application.Todos;
using Application.Todos.GetTodos;
using Mapster;
using Presentation.MinimalApi.net7.CleanArchitecture.Tests.Integration.Setup;

namespace Presentation.MinimalApi.net7.CleanArchitecture.Tests.Integration.TodosController;

public class GetTodosControllerTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    private readonly Faker<Todo> _faker = new Faker<Todo>()
        .CustomInstantiator(x => new Todo(x.Lorem.Sentence(1, 3), x.Lorem.Sentences(1, " ")))
        //.RuleFor(x => x.Title, x => x.Lorem.Sentence(1, 3))
        //.RuleFor(x => x.Description, x => x.Lorem.Sentences(1, " "))
        .RuleFor(x => x.Id, x => x.Random.Uuid())
        .RuleFor(x => x.CreatedAt, x => x.Date.Past(10))
        .RuleFor(x => x.IsCompleted, x => x.Random.Bool())
        .RuleFor(x => x.UpdatedAt,
            (f, current) => current.IsCompleted ? f.Date.Between(current.CreatedAt, DateTime.Now) : default);

    public GetTodosControllerTests(IntegrationTestFactory<Program, ApplicationDbContext> factory) : base(factory)
    {
        Randomizer.Seed = new Random(5425479);
        _client = Factory.CreateClient();
    }

    [Fact]
    public async Task GetTodos_ShouldReturnEmptyCollection_WhenCalledOnEmptyDB()
    {
        //Arrange

        //Act
        var response = await _client.GetAsync("/todos");
        //var todosResponse = await _client.GetFromJsonAsync<GetTodosQueryResponse[]>("/todos");

        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todosResponse = await response.Content.ReadFromJsonAsync<GetTodosQueryResponse[]>();
        todosResponse.Should().NotBeNull();
        todosResponse!.Length.Should().Be(0);
    }

    [Fact]
    public async Task GetTodos_ShouldReturnAll_WhenCalledOnNonEmptyDB()
    {
        //Arrange
        var cnt = 5;
        var todos = _faker.Generate(cnt);
        await DbContext.Todos.AddRangeAsync(todos);
        await DbContext.SaveChangesAsync();
        var todosResponseExpected = todos.Adapt<IList<GetTodosQueryResponse>>();

        //Act
        var response = await _client.GetAsync("/todos");

        //Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todosResponse = await response.Content.ReadFromJsonAsync<GetTodosQueryResponse[]>();
        todosResponse.Should().NotBeNull();
        todosResponse!.Length.Should().Be(cnt);
        todosResponseExpected.Should().BeEquivalentTo(todosResponse);
    }
}