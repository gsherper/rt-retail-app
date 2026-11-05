using Dashboard.Api.Common;

namespace Dashboard.Api.Features.Employees;

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

/// <summary>Version is the one the client last saw; a mismatch is a 409.</summary>
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
        errors.RequiredVersion(Version);
        return errors.ToDictionary();
    }
}
