using System.ComponentModel.DataAnnotations;
using ACP.Models.Clients;
using Microsoft.AspNetCore.Identity;

namespace ACP.Models.Identity;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? ClientOrganizationId { get; set; }

    public ClientOrganization? ClientOrganization { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginUtc { get; set; }
}