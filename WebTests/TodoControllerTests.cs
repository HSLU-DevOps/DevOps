using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Moq;
using WebAPI.Controllers;
using WebAPI.Features;
using WebAPI.Persistence;
using Assert = Xunit.Assert;

namespace WebTests;

public class TodoControllerTests
{
    private readonly Mock<IFeatureManager> _mockFeatureManager = new();

    private static DbContextOptions<TodoDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetById_FeatureEnabled_ReturnsTodo()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodo = new Todo(Guid.NewGuid(), "Test Todo", "Test Description", false, DateTime.Today, null);
        await using var context = new TodoDbContext(options);
        context.Todos.Add(testTodo);
        await context.SaveChangesAsync();

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Read)).ReturnsAsync(true);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.GetById(testTodo.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTodo = Assert.IsType<Todo>(okResult.Value);
        Assert.Equal(testTodo.Id, returnedTodo.Id);
        Assert.Equal(testTodo.Title, returnedTodo.Title);
        Assert.Equal(testTodo.Description, returnedTodo.Description);
        Assert.Equal(testTodo.Completed, returnedTodo.Completed);
        Assert.Equal(testTodo.Created, returnedTodo.Created);
    }

    [Fact]
    public async Task GetById_TodoNotFound_ReturnsNotFound()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodoId = Guid.NewGuid();
        await using var context = new TodoDbContext(options);

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Read)).ReturnsAsync(true);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.GetById(testTodoId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(testTodoId, notFoundResult.Value);
    }

    [Fact]
    public async Task GetById_FeatureDisabled_Returns501NotImplemented()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodo = new Todo(Guid.NewGuid(), "Test Todo", "Test Description", false, DateTime.Today, null);
        await using var context = new TodoDbContext(options);
        context.Todos.Add(testTodo);
        await context.SaveChangesAsync();

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Read)).ReturnsAsync(false);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.GetById(testTodo.Id);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status501NotImplemented, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetAll_FeatureEnabled_ReturnsTodos()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodos = new List<Todo>
        {
            new(Guid.NewGuid(), "Test Todo 1", "Test Description 1", false, DateTime.Today, null),
            new(Guid.NewGuid(), "Test Todo 2", "Test Description 2", true, DateTime.Today, null)
        };
        await using var context = new TodoDbContext(options);
        context.Todos.AddRange(testTodos);
        await context.SaveChangesAsync();

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Read)).ReturnsAsync(true);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var todos = Assert.IsAssignableFrom<IEnumerable<Todo>>(okResult.Value);
        Assert.Equal(2, todos.Count());
    }

    [Fact]
    public async Task GetAll_FeatureDisabled_Returns501NotImplemented()
    {
        // Arrange
        var options = GetDbContextOptions();
        await using var context = new TodoDbContext(options);

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Read)).ReturnsAsync(false);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status501NotImplemented, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Create_FeatureEnabled_CreatesTodo_ReturnsGuid()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodo = new Todo(Guid.NewGuid(), "Test Todo", "Test Description", false, DateTime.Today, null);
        await using var context = new TodoDbContext(options);
        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Create)).ReturnsAsync(true);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.Create(testTodo);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedId = Assert.IsType<Guid>(okResult.Value);
        Assert.NotEqual(Guid.Empty, returnedId);

        // Verify the "to do" was added to the database.
        var addedTodo = await context.Todos.FirstOrDefaultAsync(t => t.Id == returnedId);
        Assert.NotNull(addedTodo);
        Assert.Equal(testTodo.Title, addedTodo.Title);
        Assert.Equal(testTodo.Description, addedTodo.Description);
        Assert.Equal(testTodo.Done, addedTodo.Done);
    }


    [Fact]
    public async Task Create_FeatureDisabled_Returns501NotImplemented()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodo = new Todo(Guid.NewGuid(), "Test Todo", "Test Description", false, DateTime.Today, null);
        await using var context = new TodoDbContext(options);

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Create)).ReturnsAsync(false);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.Create(testTodo);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status501NotImplemented, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Delete_FeatureEnabled_ValidId_DeletesTodo()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodo = new Todo(Guid.NewGuid(), "Test Todo", "Test Description", false, DateTime.Today, null);
        await using var context = new TodoDbContext(options);
        context.Todos.Add(testTodo);
        await context.SaveChangesAsync();

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Mutate)).ReturnsAsync(true);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.Delete(testTodo.Id);

        // Assert
        var okResult = Assert.IsType<OkResult>(result);

        // Verify the Todo was deleted
        var deletedTodo = await context.Todos.FindAsync(testTodo.Id);
        Assert.Null(deletedTodo);
    }

    [Fact]
    public async Task Delete_FeatureEnabled_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodoId = Guid.NewGuid();
        await using var context = new TodoDbContext(options);

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Mutate)).ReturnsAsync(true);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.Delete(testTodoId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(testTodoId, notFoundResult.Value);
    }

    [Fact]
    public async Task Delete_FeatureDisabled_Returns501NotImplemented()
    {
        // Arrange
        var options = GetDbContextOptions();
        var testTodoId = Guid.NewGuid();
        await using var context = new TodoDbContext(options);

        _mockFeatureManager.Setup(fm => fm.IsEnabledAsync(FeatureFlags.Mutate)).ReturnsAsync(false);

        var controller = new TodoController(context, _mockFeatureManager.Object);

        // Act
        var result = await controller.Delete(testTodoId);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status501NotImplemented, statusCodeResult.StatusCode);
    }
}