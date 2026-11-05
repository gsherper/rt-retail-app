namespace Dashboard.Api.Common;

/// <summary>First name, middle initial and last name rules shared by customers and employees.</summary>
public static class PersonNameRules
{
    public const int MaxNameLength = 50;

    public static void Validate(
        ValidationErrors errors, string? firstName, string? middleInitial, string? lastName)
    {
        errors.RequiredText("firstName", "First name", firstName, MaxNameLength);
        errors.RequiredText("lastName", "Last name", lastName, MaxNameLength);
        if (middleInitial?.Trim().Length > 1)
            errors.Add("middleInitial", "Middle initial must be a single character.");
    }

    /// <summary>Blank becomes null; otherwise one uppercase letter.</summary>
    public static string? NormalizeInitial(string? middleInitial)
    {
        var trimmed = middleInitial?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.ToUpperInvariant();
    }
}
