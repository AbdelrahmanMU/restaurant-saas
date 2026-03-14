using Microsoft.EntityFrameworkCore;
using RestaurantSaas.Api.Domain.Entities;
using RestaurantSaas.Api.Domain.Lookups;

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

    // ── Menu module ────────────────────────────────────────────────────────────
    public DbSet<MenuSection> MenuSections => Set<MenuSection>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<ModifierOption> ModifierOptions => Set<ModifierOption>();
    public DbSet<ProductVariantModifierGroup> ProductVariantModifierGroups => Set<ProductVariantModifierGroup>();
    public DbSet<Bundle> Bundles => Set<Bundle>();
    public DbSet<BundleSlot> BundleSlots => Set<BundleSlot>();
    public DbSet<BundleSlotChoice> BundleSlotChoices => Set<BundleSlotChoice>();
    public DbSet<BranchProductVariant> BranchProductVariants => Set<BranchProductVariant>();

    // ── Lookup tables ──────────────────────────────────────────────────────────
    public DbSet<ProductTypeLookup> ProductTypes => Set<ProductTypeLookup>();
    public DbSet<SelectionTypeLookup> SelectionTypes => Set<SelectionTypeLookup>();
    public DbSet<OrderStatusLookup> OrderStatuses => Set<OrderStatusLookup>();
    public DbSet<FulfillmentTypeLookup> FulfillmentTypes => Set<FulfillmentTypeLookup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Soft-delete filter ─────────────────────────────────────────────────
        // BundleSlots use IsActive for soft delete
        modelBuilder.Entity<BundleSlot>().HasQueryFilter(s => s.IsActive);

        // ── Core org entities ──────────────────────────────────────────────────

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
            e.HasOne(x => x.Restaurant).WithMany(r => r.Branches)
                .HasForeignKey(x => x.RestaurantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PhoneNumber).IsUnique();
            e.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200);
            e.HasMany(x => x.UserRoles).WithOne(r => r.User).HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Role).HasConversion<string>();
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
            e.HasOne(x => x.Restaurant).WithOne(r => r.Settings)
                .HasForeignKey<RestaurantSettings>(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Driver>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DriverAttribute>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Driver).WithMany(d => d.Attributes).HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DriverDocument>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Driver).WithMany(d => d.Documents).HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DriverBranchAccess>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Driver).WithMany(d => d.BranchAccess).HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.FulfillmentType).HasConversion<string>();
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Branch).WithMany(b => b.Orders).HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderEventLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FromStatus).HasConversion<string>();
            e.Property(x => x.ToStatus).HasConversion<string>();
            e.HasOne(x => x.Order).WithMany(o => o.EventLogs).HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DriverActiveOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.DriverId).IsUnique();
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Menu module ────────────────────────────────────────────────────────

        modelBuilder.Entity<MenuSection>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => new { x.BranchId, x.Name }).IsUnique();
            e.HasOne(x => x.Restaurant).WithMany().HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
            e.Property(x => x.Type).HasConversion<string>();
            e.HasOne(x => x.Restaurant).WithMany().HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.MenuSection).WithMany(s => s.Products)
                .HasForeignKey(x => x.MenuSectionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Sku).HasMaxLength(100);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasOne(x => x.Product).WithMany(p => p.Variants)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ModifierGroup>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.SelectionType).HasConversion<string>();
            e.HasOne(x => x.Restaurant).WithMany().HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ModifierOption>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.PriceDelta).HasPrecision(18, 2);
            e.HasOne(x => x.ModifierGroup).WithMany(g => g.Options)
                .HasForeignKey(x => x.ModifierGroupId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariantModifierGroup>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ProductVariantId, x.ModifierGroupId }).IsUnique();
            e.HasOne(x => x.ProductVariant).WithMany(v => v.ModifierGroups)
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ModifierGroup).WithMany(g => g.ProductVariants)
                .HasForeignKey(x => x.ModifierGroupId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Bundle>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProductId).IsUnique();
            e.HasOne(x => x.Product).WithOne(p => p.Bundle)
                .HasForeignKey<Bundle>(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BundleSlot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.HasOne(x => x.Bundle).WithMany(b => b.Slots)
                .HasForeignKey(x => x.BundleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BundleSlotChoice>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PriceDelta).HasPrecision(18, 2);
            e.HasIndex(x => new { x.BundleSlotId, x.ProductVariantId }).IsUnique();
            e.HasOne(x => x.BundleSlot).WithMany(s => s.Choices)
                .HasForeignKey(x => x.BundleSlotId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ProductVariant).WithMany(v => v.BundleSlotChoices)
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BranchProductVariant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PriceOverride).HasPrecision(18, 2);
            e.HasIndex(x => new { x.BranchId, x.ProductVariantId }).IsUnique();
            e.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ProductVariant).WithMany(v => v.BranchAvailability)
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── Lookup tables ──────────────────────────────────────────────────────

        modelBuilder.Entity<ProductTypeLookup>(e =>
        {
            e.HasKey(x => x.Name);
            e.Property(x => x.Name).HasMaxLength(50);
            e.ToTable("ProductTypes");
            e.HasData(
                new ProductTypeLookup { Name = "Simple" },
                new ProductTypeLookup { Name = "VariantBased" },
                new ProductTypeLookup { Name = "Customizable" },
                new ProductTypeLookup { Name = "Bundle" }
            );
        });

        modelBuilder.Entity<SelectionTypeLookup>(e =>
        {
            e.HasKey(x => x.Name);
            e.Property(x => x.Name).HasMaxLength(50);
            e.ToTable("SelectionTypes");
            e.HasData(
                new SelectionTypeLookup { Name = "Single" },
                new SelectionTypeLookup { Name = "Multiple" }
            );
        });

        modelBuilder.Entity<OrderStatusLookup>(e =>
        {
            e.HasKey(x => x.Name);
            e.Property(x => x.Name).HasMaxLength(50);
            e.ToTable("OrderStatuses");
            e.HasData(
                new OrderStatusLookup { Name = "PendingAcceptance" },
                new OrderStatusLookup { Name = "Accepted" },
                new OrderStatusLookup { Name = "Preparing" },
                new OrderStatusLookup { Name = "ReadyForDispatch" },
                new OrderStatusLookup { Name = "PendingHandover" },
                new OrderStatusLookup { Name = "PickedUp" },
                new OrderStatusLookup { Name = "Delivered" },
                new OrderStatusLookup { Name = "Completed" },
                new OrderStatusLookup { Name = "Cancelled" }
            );
        });

        modelBuilder.Entity<FulfillmentTypeLookup>(e =>
        {
            e.HasKey(x => x.Name);
            e.Property(x => x.Name).HasMaxLength(50);
            e.ToTable("FulfillmentTypes");
            e.HasData(
                new FulfillmentTypeLookup { Name = "Delivery" },
                new FulfillmentTypeLookup { Name = "Pickup" },
                new FulfillmentTypeLookup { Name = "DineIn" }
            );
        });
    }
}
