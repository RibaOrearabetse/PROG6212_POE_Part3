using Contract_Monthly_Claim_System__CMCS_.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System__CMCS_.Data;

public class CmcsDbContext : DbContext
{
    public CmcsDbContext(DbContextOptions<CmcsDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<SupportingDocument> SupportingDocuments => Set<SupportingDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}

