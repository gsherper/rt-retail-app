using Dashboard.Api.Common;

namespace Dashboard.Api.Features.Customers;

/// <summary>A customer as returned by the hub and REST and pushed in <c>CustomerChanged</c> events.</summary>
public sealed record CustomerDto(
    int CustomerID,
    string FirstName,
    string? MiddleInitial,
    string LastName,
    long Version);

public sealed record CreateCustomerRequest(string? FirstName, string? MiddleInitial, string? LastName)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new ValidationErrors();
        PersonNameRules.Validate(errors, FirstName, MiddleInitial, LastName);
        return errors.ToDictionary();
    }
}

/// <summary>Version is the one the client last saw; a mismatch means someone else saved first.</summary>
public sealed record UpdateCustomerRequest(
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
