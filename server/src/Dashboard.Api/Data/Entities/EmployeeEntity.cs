namespace Dashboard.Api.Data.Entities;

public sealed class EmployeeEntity
{
    public int Id { get; set; }

    public required string FirstName { get; set; }

    public string? MiddleInitial { get; set; }

    public required string LastName { get; set; }

    /// <summary>Row version; set at startup and changed on every write.</summary>
    public long Version { get; set; }
}
