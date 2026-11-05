namespace Dashboard.Api.Common;

/// <summary>Collects field errors in the shape <c>Results.ValidationProblem</c> expects.</summary>
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public bool HasErrors => _errors.Count > 0;

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
            _errors[field] = messages = [];
        messages.Add(message);
    }

    public void RequiredText(string field, string label, string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            Add(field, $"{label} is required.");
        else if (value.Trim().Length > maxLength)
            Add(field, $"{label} must be {maxLength} characters or fewer.");
    }

    public void RequiredId(string field, string label, int? value)
    {
        if (value is null)
            Add(field, $"{label} is required.");
        else if (value <= 0)
            Add(field, $"{label} must be a positive number.");
    }

    public void Range(string field, string label, int? value, int min, int max)
    {
        if (value is null)
            Add(field, $"{label} is required.");
        else if (value < min || value > max)
            Add(field, $"{label} must be between {min} and {max}.");
    }

    public void RequiredVersion(long? version)
    {
        if (version is null)
            Add("version", "Version is required.");
    }

    public Dictionary<string, string[]> ToDictionary() =>
        _errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
}
