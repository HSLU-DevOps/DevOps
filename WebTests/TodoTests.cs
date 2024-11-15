using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Controllers;
using WebAPI.Persistence;

namespace WebTests;

public class Tests
{
    private TodoDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test
            .Options;

        var dbContext = new TodoDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    [Test]
    public void Get_ShouldReturnOkResult()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext();
        var controller = new TodoController(dbContext);

        // Act
        var result = controller.Get();

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
    }

    [Test]
    public void GetAllTodos_WhenTodosExist_ShouldReturnAllTodos()
    {
        // Arrange
        using var dbContext = GetInMemoryDbContext();
        var todos = new List<Todo>
        {
            new(Guid.NewGuid(), "Test Todo 1", "Description", true, DateTime.Now - TimeSpan.FromDays(1), DateTime.Now),
            new(Guid.NewGuid(), "Test Todo 2", "Second description", false, DateTime.Now.AddDays(-2), null)
        };
        dbContext.Todos.AddRange(todos);
        dbContext.SaveChanges();

        var controller = new TodoController(dbContext);

        // Act
        var result = controller.GetAllTodos();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedTodos = okResult.Value as List<Todo>;
        Assert.That(returnedTodos, Is.Not.Null);
        Assert.That(returnedTodos, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetAllTodos_WhenNoTodosExist_ShouldReturnEmptyList()
    {
        //Test CI
        // Arrange
        using var dbContext = GetInMemoryDbContext();
        var controller = new TodoController(dbContext);

        // Act
        var result = controller.GetAllTodos();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedTodos = okResult.Value as List<Todo>;
        Assert.That(returnedTodos, Is.Not.Null);
        Assert.That(returnedTodos, Is.Empty);
    }
}