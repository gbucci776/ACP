using ACP.Models.Clients;
using ACP.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ACP.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClientOrganization> ClientOrganizations =>
        Set<ClientOrganization>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ClientOrganization>(entity =>
        {
            entity.ToTable("ClientOrganizations");

            entity.HasKey(organization => organization.Id);

            entity.Property(organization => organization.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(organization => organization.ClientNumber)
                .HasMaxLength(50);

            entity.Property(organization => organization.IsActive)
                .IsRequired();

            entity.Property(organization => organization.CreatedUtc)
                .IsRequired();

            entity.HasIndex(organization => organization.CompanyName);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(user => user.ClientOrganization)
                .WithMany(organization => organization.Users)
                .HasForeignKey(user => user.ClientOrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(user => user.ClientOrganizationId);
        });
    }
}