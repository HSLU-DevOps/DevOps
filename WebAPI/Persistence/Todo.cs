namespace WebAPI.Persistence;

public record Todo(Guid Id, string Title, string Description, bool Done, DateTime Created, DateTime? Completed);