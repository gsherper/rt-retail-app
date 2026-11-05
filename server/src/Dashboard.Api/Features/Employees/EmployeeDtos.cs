using Dashboard.Api.Common;

namespace Dashboard.Api.Features.Employees;

/// <summary>An employee as returned by the hub and REST and pushed in <c>EmployeeChanged</c> events.</summary>
public sealed record EmployeeDto(
    int EmployeeID,
    string FirstName,
    string? MiddleInitial,
    string LastName,
    long Version);

public sealed record CreateEmployeeRequest(string? FirstName, string? MiddleInitial, string? LastName)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        PersonNameRules.Validate(errors, FirstName, MiddleInitial, LastName);
        return errors.ToDictionary();
    }
}

/// <summary>Version is the one the client last saw; a mismatch means someone else saved first.</summary>
public sealed record UpdateEmployeeRequest(
    string? FirstName,
    string? MiddleInitial,
    string? LastName,
    long? Version)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        PersonNameRules.Validate(errors, FirstName, MiddleInitial, LastName);
        if (Version is null)
            errors.Add("version", "Version is required.");
        return errors.ToDictionary();
    }
}
