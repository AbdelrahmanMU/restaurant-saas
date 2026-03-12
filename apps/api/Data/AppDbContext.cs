using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Domain.Entities;

namespace RestaurantSaas.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserInvite> UserInvites => Set<UserInvite>();
    public DbSet<RestaurantSettings> RestaurantSettings => Set<RestaurantSettings>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DriverAttribute> DriverAttributes => Set<DriverAttribute>();
    public DbSet<DriverDocument> DriverDocuments => Set<DriverDocument>();
    public DbSet<DriverBranchAccess> DriverBranchAccesses => Set<DriverBranchAccess>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderEventLog> OrderEventLogs => Set<OrderEventLog>();
    public DbSet<DriverActiveOrder> DriverActiveOrders => Set<DriverActiveOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Restaurant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Branch>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Restaurant).WithMany(r => r.Branches).HasForeignKey(x => x.RestaurantId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PhoneNumber).IsUnique();
            e.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200);
            e.HasMany(x => x.UserRoles).WithOne(r => r.User).HasForeignKey(r => r.UserId);
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Role).HasConversion<string>();
            // prevent duplicate role assignments for same user+role+branch
            e.HasIndex(x => new { x.UserId, x.Role, x.BranchId }).IsUnique();
        });

        modelBuilder.Entity<UserInvite>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Role).HasConversion<string>();
        });

        modelBuilder.Entity<RestaurantSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Restaurant).WithOne(r => r.Settings).HasForeignKey<RestaurantSettings>(x => x.RestaurantId);
        });

        modelBuilder.Entity<Driver>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<DriverAttribute>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Driver).WithMany(d => d.Attributes).HasForeignKey(x => x.DriverId);
        });

        modelBuilder.Entity<DriverDocument>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Driver).WithMany(d => d.Documents).HasForeignKey(x => x.DriverId);
        });

        modelBuilder.Entity<DriverBranchAccess>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Driver).WithMany(d => d.BranchAccess).HasForeignKey(x => x.DriverId);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.FulfillmentType).HasConversion<string>();
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Branch).WithMany(b => b.Orders).HasForeignKey(x => x.BranchId);
        });

        modelBuilder.Entity<OrderEventLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FromStatus).HasConversion<string>();
            e.Property(x => x.ToStatus).HasConversion<string>();
            e.HasOne(x => x.Order).WithMany(o => o.EventLogs).HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<DriverActiveOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.DriverId).IsUnique();
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId);
            e.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId);
        });
    }
}
