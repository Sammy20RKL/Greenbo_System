using AspnetCoreMvcFull.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

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
    public DbSet<User> Users { get; set; }
    public DbSet<WorkFlow> WorkFlow { get; set; }
      public DbSet<WorkOrderStepEffort> WorkOrderStepEffort { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Company>().ToTable("Companies");
      modelBuilder.Entity<Project>().ToTable("Projects");
      modelBuilder.Entity<ProjectDesign>().ToTable("ProjectDesign");
      modelBuilder.Entity<MaterialList>().ToTable("MaterialList");
      modelBuilder.Entity<MachineRegistration>().ToTable("MachineRegistration");
      modelBuilder.Entity<MachineAllocation>().ToTable("MachineAllocation");
      modelBuilder.Entity<OrderOperation>().ToTable("OrderOperations");
      modelBuilder.Entity<User>().ToTable("Users");
      modelBuilder.Entity<WorkFlow>().ToTable("WorkFlow");
      modelBuilder.Entity<WorkOrderStepEffort>().ToTable("WorkOrderStepEffort");

      modelBuilder.Entity<MaterialList>(entity =>
      {
        entity.Property(e => e.TotalWeight).HasPrecision(18, 4);
        entity.Property(e => e.UnitWeight).HasPrecision(18, 4);
      });
      


      base.OnModelCreating(modelBuilder);
    }
   
  }
}
