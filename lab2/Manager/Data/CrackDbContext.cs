// Manager/Data/CrackDbContext.cs

using Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace Manager.Data;

public class CrackDbContext(DbContextOptions<CrackDbContext> options) : DbContext(options)
{
    public DbSet<CrackRequest> Requests => Set<CrackRequest>();
    public DbSet<CrackTask> Tasks => Set<CrackTask>();
    public DbSet<FoundWord> FoundWords => Set<FoundWord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CrackRequest>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasMany(r => r.Tasks).WithOne(t => t.Request).HasForeignKey(t => t.RequestId);
            b.HasMany(r => r.FoundWords).WithOne(f => f.Request).HasForeignKey(f => f.RequestId);
        });

        modelBuilder.Entity<CrackTask>(b =>
        {
            b.HasKey(t => t.TaskId);
            b.HasIndex(t => new { t.RequestId, t.PartNumber }).IsUnique();
        });

        modelBuilder.Entity<FoundWord>(b =>
        {
            b.HasKey(f => f.Id);
            b.HasIndex(f => new { f.RequestId, f.Word }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}