namespace Dashboard.Api.Features.Customers;

/// <summary>Makes up customer details for demos and for testing live updates.</summary>
internal static class RandomCustomer
{
    private static readonly string[] FirstNames =
    [
        "Olivia", "Liam", "Emma", "Noah", "Ava", "Oliver", "Sophia", "Elijah",
        "Isabella", "Lucas", "Mia", "Mason", "Amelia", "Ethan", "Harper", "Logan",
        "Evelyn", "Aiden", "Abigail", "Carter", "Emily", "Grayson", "Ella", "Wyatt",
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson",
        "Taylor", "Moore", "Allen", "Young", "King", "Wright", "Scott", "Green",
        "Baker", "Adams", "Nelson", "Hill", "Campbell", "Mitchell", "Roberts", "Carter",
    ];

    public static CreateCustomerRequest Create()
    {
        var random = Random.Shared;

        // About a third of customers have no middle initial, like the seed data
        var middleInitial = random.Next(3) == 0
            ? null
            : ((char)('A' + random.Next(26))).ToString();

        return new CreateCustomerRequest(
            FirstNames[random.Next(FirstNames.Length)],
            middleInitial,
            LastNames[random.Next(LastNames.Length)]);
    }
}
