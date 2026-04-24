using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ArgosApi.Models;
using ArgosApi.Enums;

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
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftDetail> ShiftDetails => Set<ShiftDetail>();
    public DbSet<EmployeeShift> EmployeeShifts => Set<EmployeeShift>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BiometricTemplate> BiometricTemplates => Set<BiometricTemplate>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var enumsAsString = new HashSet<Type>
        {
            typeof(DocumentType),
            typeof(Manufacturer),
            typeof(TaxIdType),
            typeof(BiometricType)
        };


        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var propertyType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

                if (!propertyType.IsEnum || !enumsAsString.Contains(propertyType))
                {
                    continue;
                }

                var converterType = typeof(EnumToStringConverter<>).MakeGenericType(propertyType);
                var converter = (ValueConverter)Activator.CreateInstance(converterType, new object?[] { null })!;
                property.SetValueConverter(converter);
            }
        }

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

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
