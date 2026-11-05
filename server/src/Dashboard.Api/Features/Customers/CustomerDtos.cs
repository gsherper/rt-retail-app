using Dashboard.Api.Common;

namespace Dashboard.Api.Features.Customers;

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

/// <summary>Version is the one the client last saw; a mismatch is a 409.</summary>
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
        errors.RequiredVersion(Version);
        return errors.ToDictionary();
    }
}
