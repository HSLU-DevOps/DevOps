using Microsoft.AspNetCore.Mvc;
using WebAPI.Persistence;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoController(TodoDbContext dbContext) : ControllerBase
{
    [HttpGet("[action]")]
    public IActionResult Get()
    {
        return Ok();
    }

    [HttpGet("[action]")]
    public IActionResult GetAllTodos()
    {
        return Ok(dbContext.Todos.ToList());
    }
}