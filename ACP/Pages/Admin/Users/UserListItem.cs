namespace ACP.Pages.Admin.Users;

public sealed class UserListItem
{
    public string Id { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool EmailConfirmed { get; init; }

    public bool IsActive { get; init; }

    public string? CompanyName { get; set; }

    public int? ClientOrganizationId { get; set; }

    public bool IsClientUser { get; set; }

    public DateTime CreatedUtc { get; init; }

    public DateTime? LastLoginUtc { get; init; }

    public IReadOnlyList<string> Roles { get; init; } =
        Array.Empty<string>();

    public string DisplayName
    {
        get
        {
            var fullName = $"{FirstName} {LastName}".Trim();

            return string.IsNullOrWhiteSpace(fullName)
                ? Email
                : fullName;
        }
    }
}