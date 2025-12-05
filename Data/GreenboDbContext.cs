using AspnetCoreMvcFull.Models;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Data
{
  public class GreenboDbContext : DbContext
  {
    public GreenboDbContext(DbContextOptions<GreenboDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectDesign> ProjectDesigns { get; set; }
    public DbSet<MaterialList> MaterialLists { get; set; }
    public DbSet<MachineRegistration> MachineRegistrations { get; set; }
    public DbSet<MachineAllocation> MachineAllocations { get; set; }
    public DbSet<OrderOperation> OrderOperations { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Company>().ToTable("Companies");
      modelBuilder.Entity<Project>().ToTable("Projects");
      modelBuilder.Entity<ProjectDesign>().ToTable("ProjectDesign");
      modelBuilder.Entity<MaterialList>().ToTable("MaterialList");
      modelBuilder.Entity<MachineRegistration>().ToTable("MachineRegistration");
      modelBuilder.Entity<MachineAllocation>().ToTable("MachineAllocation");
      modelBuilder.Entity<OrderOperation>().ToTable("OrderOperations");

      base.OnModelCreating(modelBuilder);
    }
  }
}
