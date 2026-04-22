using Microsoft.EntityFrameworkCore;
using ArgosApi.Models;

namespace ArgosApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyAlias> CompanyAliases => Set<CompanyAlias>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ClockProfile> ClockProfiles => Set<ClockProfile>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftDetail> ShiftDetails => Set<ShiftDetail>();
    public DbSet<EmployeeShift> EmployeeShifts => Set<EmployeeShift>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BiometricTemplate> BiometricTemplates => Set<BiometricTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CompanyAlias>()
            .HasMany(e => e.Employees)
            .WithOne(e => e.Alias)
            .HasForeignKey(e => e.AliasId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Device)
                .WithMany(e => e.Attendances)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

        });
    }
}
