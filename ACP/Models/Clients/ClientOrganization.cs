using System.ComponentModel.DataAnnotations;
using ACP.Models.Identity;

namespace ACP.Models.Clients;

public class ClientOrganization
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ClientNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationUser> Users { get; set; } =
        new List<ApplicationUser>();
}