using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using WebAPI.Features;
using WebAPI.Persistence;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TodoController(TodoDbContext dbContext, IFeatureManager featureManager) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!await featureManager.IsEnabledAsync(FeatureFlags.Read))
            return StatusCode(StatusCodes.Status501NotImplemented);
        var todo = await dbContext.Todos.SingleOrDefaultAsync(x => x.Id == id);
        return todo == null ? NotFound(id) : Ok(todo);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!await featureManager.IsEnabledAsync(FeatureFlags.Read))
            return StatusCode(StatusCodes.Status501NotImplemented);
        return Ok(dbContext.Todos.ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Todo todo)
    {
        if (!await featureManager.IsEnabledAsync(FeatureFlags.Create))
            return StatusCode(StatusCodes.Status501NotImplemented);
        dbContext.Todos.Add(todo with { Id = Guid.NewGuid(), Created = DateTime.Today });
        return Ok(await dbContext.SaveChangesAsync());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await featureManager.IsEnabledAsync(FeatureFlags.Mutate))
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        var todo = await dbContext.Todos.FindAsync(id);
        if (todo == null)
        {
            return NotFound(id);
        }

        dbContext.Todos.Remove(todo);
        await dbContext.SaveChangesAsync();
        return Ok();
    }
}